using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services
{
    public class RezervacijaService : IRezervacijaService
    {
        private const int DefaultSpaCentarId = 1;
        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public RezervacijaService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RezervacijaDTO>> GetAsync(
            int? korisnikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false,
            int? zaposlenikId = null)
        {
            var query = _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Include(r => r.RezervacijaOprema)
                .AsQueryable();

            if (!includeOtkazane)
            {
                query = query.Where(r => !r.IsOtkazana);
            }

            if (korisnikId is int id)
            {
                query = query.Where(r => r.KorisnikId == id);
            }

            if (zaposlenikId is int zid)
            {
                query = query.Where(r => r.ZaposlenikId == zid);
            }

            if (datum.HasValue)
            {
                // MVP: filtrira po datumu (bez vremena)
                var date = datum.Value.Date;
                query = query.Where(r => r.DatumRezervacije.Date == date);
            }

            if (isPotvrdjena.HasValue)
            {
                query = query.Where(r => r.IsPotvrdjena == isPotvrdjena.Value);
            }

            var list = await query
                .OrderByDescending(r => r.DatumRezervacije)
                .ToListAsync();

            return await MapAndEnrichAsync(list);
        }

        public async Task<IEnumerable<RezervacijaDTO>> GetForZaposlenikAsync(
            int zaposlenikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false)
        {
            var query = _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Include(r => r.RezervacijaOprema)
                .Where(r => r.ZaposlenikId == zaposlenikId)
                .AsQueryable();

            if (!includeOtkazane)
            {
                query = query.Where(r => !r.IsOtkazana);
            }

            if (datum.HasValue)
            {
                var date = datum.Value.Date;
                query = query.Where(r => r.DatumRezervacije.Date == date);
            }

            if (isPotvrdjena.HasValue)
            {
                query = query.Where(r => r.IsPotvrdjena == isPotvrdjena.Value);
            }

            var list = await query
                .OrderByDescending(r => r.DatumRezervacije)
                .ToListAsync();

            return await MapAndEnrichAsync(list);
        }

        public async Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto)
        {
            // Validate working hours (SpaCentarId = 1)
            var hours = await GetWorkingHoursAsync(dto.DatumRezervacije);
            if (hours.IsClosed)
                throw new InvalidOperationException("Spa centar je zatvoren za odabrani dan.");

            if (!IsWithinWorkingHours(dto.DatumRezervacije, hours.OpenMin, hours.CloseMin))
                throw new InvalidOperationException("Termin je van radnog vremena spa centra.");

            // Validate selected room (optional) + equipment availability for the slot.
            Prostorija? prostorija = null;
            if (dto.ProstorijaId.HasValue)
            {
                prostorija = await _context.Prostorije
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.ProstorijaId.Value &&
                        x.IsAktivna &&
                        x.SpaCentarId == DefaultSpaCentarId);
                if (prostorija == null)
                    throw new InvalidOperationException("Prostorija nije dostupna.");

                var roomTaken = await _context.Rezervacije
                    .AsNoTracking()
                    .AnyAsync(r =>
                        !r.IsOtkazana &&
                        r.ProstorijaId == dto.ProstorijaId.Value &&
                        r.DatumRezervacije == dto.DatumRezervacije);
                if (roomTaken)
                    throw new InvalidOperationException("Prostorija je već zauzeta za odabrani termin.");
            }

            var opremaItems = (dto.Oprema ?? new List<RezervacijaOpremaItemDTO>())
                .Where(x => x.OpremaId > 0 && x.Kolicina > 0)
                .GroupBy(x => x.OpremaId)
                .Select(g => new RezervacijaOpremaItemDTO { OpremaId = g.Key, Kolicina = g.Sum(x => x.Kolicina) })
                .ToList();

            if (opremaItems.Count > 0)
            {
                var opremaIds = opremaItems.Select(x => x.OpremaId).ToList();
                var opremaInDb = await _context.Oprema
                    .AsNoTracking()
                    .Where(x => x.SpaCentarId == DefaultSpaCentarId && x.IsIspravna && opremaIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Kolicina })
                    .ToListAsync();

                if (opremaInDb.Count != opremaIds.Count)
                    throw new InvalidOperationException("Dio opreme nije dostupan.");

                var reserved = await _context.RezervacijeOprema
                    .AsNoTracking()
                    .Where(x =>
                        !x.Rezervacija.IsOtkazana &&
                        x.Rezervacija.DatumRezervacije == dto.DatumRezervacije &&
                        opremaIds.Contains(x.OpremaId))
                    .GroupBy(x => x.OpremaId)
                    .Select(g => new { OpremaId = g.Key, Qty = g.Sum(x => x.Kolicina) })
                    .ToListAsync();

                var reservedMap = reserved.ToDictionary(x => x.OpremaId, x => x.Qty);
                foreach (var item in opremaItems)
                {
                    var total = opremaInDb.First(x => x.Id == item.OpremaId).Kolicina;
                    var already = reservedMap.TryGetValue(item.OpremaId, out var v) ? v : 0;
                    if (already + item.Kolicina > total)
                        throw new InvalidOperationException("Nema dovoljno opreme za odabrani termin.");
                }
            }

            var entity = new Rezervacija
            {
                KorisnikId = korisnikId,
                UslugaId = dto.UslugaId,
                ZaposlenikId = dto.ZaposlenikId,
                DatumRezervacije = dto.DatumRezervacije,
                IsPotvrdjena = false,
                IsPlacena = false,
                ProstorijaId = dto.ProstorijaId
            };

            _context.Rezervacije.Add(entity);
            await _context.SaveChangesAsync();

            if (opremaItems.Count > 0)
            {
                foreach (var item in opremaItems)
                {
                    _context.RezervacijeOprema.Add(new RezervacijaOprema
                    {
                        RezervacijaId = entity.Id,
                        OpremaId = item.OpremaId,
                        Kolicina = item.Kolicina,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Dohvati relacije za DTO mapiranje (Korisnik/Usluga/Zaposlenik).
            var created = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Include(r => r.RezervacijaOprema)
                .FirstAsync(r => r.Id == entity.Id);

            return await MapSingleAndEnrichAsync(created);
        }

        public async Task<RezervacijaDTO?> EditAsync(int rezervacijaId, RezervacijaEditDTO dto)
        {
            var entity = await _context.Rezervacije
                .Include(r => r.RezervacijaOprema)
                .FirstOrDefaultAsync(r => r.Id == rezervacijaId);
            if (entity == null) return null;
            if (entity.IsOtkazana) throw new InvalidOperationException("Otkazana rezervacija se ne može mijenjati.");
            if (entity.IsPlacena) throw new InvalidOperationException("Plaćena rezervacija se ne može mijenjati.");

            // Validate working hours
            var hours = await GetWorkingHoursAsync(dto.DatumRezervacije);
            if (hours.IsClosed) throw new InvalidOperationException("Spa centar je zatvoren za odabrani dan.");
            if (!IsWithinWorkingHours(dto.DatumRezervacije, hours.OpenMin, hours.CloseMin))
                throw new InvalidOperationException("Termin je van radnog vremena spa centra.");

            // Validate room (exclude current reservation)
            if (dto.ProstorijaId.HasValue)
            {
                var prostorija = await _context.Prostorije
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.ProstorijaId.Value &&
                        x.IsAktivna &&
                        x.SpaCentarId == DefaultSpaCentarId);
                if (prostorija == null) throw new InvalidOperationException("Prostorija nije dostupna.");

                var roomTaken = await _context.Rezervacije
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.Id != rezervacijaId &&
                        !r.IsOtkazana &&
                        r.ProstorijaId == dto.ProstorijaId.Value &&
                        r.DatumRezervacije == dto.DatumRezervacije);
                if (roomTaken) throw new InvalidOperationException("Prostorija je već zauzeta za odabrani termin.");
            }

            // Normalize equipment list
            var opremaItems = (dto.Oprema ?? new List<RezervacijaOpremaItemDTO>())
                .Where(x => x.OpremaId > 0 && x.Kolicina > 0)
                .GroupBy(x => x.OpremaId)
                .Select(g => new RezervacijaOpremaItemDTO { OpremaId = g.Key, Kolicina = g.Sum(x => x.Kolicina) })
                .ToList();

            if (opremaItems.Count > 0)
            {
                var opremaIds = opremaItems.Select(x => x.OpremaId).ToList();
                var opremaInDb = await _context.Oprema
                    .AsNoTracking()
                    .Where(x => x.SpaCentarId == DefaultSpaCentarId && x.IsIspravna && opremaIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.Kolicina })
                    .ToListAsync();
                if (opremaInDb.Count != opremaIds.Count)
                    throw new InvalidOperationException("Dio opreme nije dostupan.");

                // Sum reserved equipment for the slot excluding current reservation
                var reserved = await _context.RezervacijeOprema
                    .AsNoTracking()
                    .Where(x =>
                        x.RezervacijaId != rezervacijaId &&
                        !x.Rezervacija.IsOtkazana &&
                        x.Rezervacija.DatumRezervacije == dto.DatumRezervacije &&
                        opremaIds.Contains(x.OpremaId))
                    .GroupBy(x => x.OpremaId)
                    .Select(g => new { OpremaId = g.Key, Qty = g.Sum(x => x.Kolicina) })
                    .ToListAsync();

                var reservedMap = reserved.ToDictionary(x => x.OpremaId, x => x.Qty);
                foreach (var item in opremaItems)
                {
                    var total = opremaInDb.First(x => x.Id == item.OpremaId).Kolicina;
                    var already = reservedMap.TryGetValue(item.OpremaId, out var v) ? v : 0;
                    if (already + item.Kolicina > total)
                        throw new InvalidOperationException("Nema dovoljno opreme za odabrani termin.");
                }
            }

            // Apply changes
            entity.DatumRezervacije = dto.DatumRezervacije;
            entity.ZaposlenikId = dto.ZaposlenikId;
            entity.UslugaId = dto.UslugaId;
            entity.ProstorijaId = dto.ProstorijaId;

            // Replace equipment links
            if (entity.RezervacijaOprema.Count > 0)
            {
                _context.RezervacijeOprema.RemoveRange(entity.RezervacijaOprema);
            }
            foreach (var item in opremaItems)
            {
                _context.RezervacijeOprema.Add(new RezervacijaOprema
                {
                    RezervacijaId = entity.Id,
                    OpremaId = item.OpremaId,
                    Kolicina = item.Kolicina,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var updated = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Include(r => r.RezervacijaOprema)
                .FirstAsync(r => r.Id == entity.Id);
            return await MapSingleAndEnrichAsync(updated);
        }

        public async Task<bool> UpdatePotvrdjenaAsync(int rezervacijaId, bool isPotvrdjena)
        {
            var entity = await _context.Rezervacije.FirstOrDefaultAsync(r => r.Id == rezervacijaId);
            if (entity == null) return false;

            entity.IsPotvrdjena = isPotvrdjena;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePotvrdjenaForZaposlenikAsync(int rezervacijaId, int zaposlenikId, bool isPotvrdjena)
        {
            var entity = await _context.Rezervacije.FirstOrDefaultAsync(r => r.Id == rezervacijaId);
            if (entity == null) return false;
            if (entity.ZaposlenikId != zaposlenikId) return false;

            entity.IsPotvrdjena = isPotvrdjena;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DateTime>> GetAvailableSlotsAsync(int zaposlenikId, DateTime date, int slotMinutes = 60)
        {
            var day = date.Date;
            var hours = await GetWorkingHoursAsync(day);
            if (hours.IsClosed) return new List<DateTime>();

            var start = day.AddMinutes(hours.OpenMin);
            var end = day.AddMinutes(hours.CloseMin);

            var taken = await _context.Rezervacije
                .AsNoTracking()
                .Where(r =>
                    r.ZaposlenikId == zaposlenikId &&
                    !r.IsOtkazana &&
                    r.DatumRezervacije.Date == day)
                .Select(r => r.DatumRezervacije)
                .ToListAsync();

            var takenSet = taken.ToHashSet();

            var slots = new List<DateTime>();
            for (var t = start; t < end; t = t.AddMinutes(slotMinutes))
            {
                if (!takenSet.Contains(t))
                {
                    slots.Add(t);
                }
            }

            return slots;
        }

        private async Task<(bool IsClosed, int OpenMin, int CloseMin)> GetWorkingHoursAsync(DateTime date)
        {
            var d = date.Date;
            // 1=Mon..7=Sun
            var dayOfWeek = ((int)d.DayOfWeek + 6) % 7 + 1;

            var hours = await _context.RadnaVremena
                .AsNoTracking()
                .Where(x => x.SpaCentarId == DefaultSpaCentarId && x.DanUSedmici == dayOfWeek)
                .Select(x => new { x.IsClosed, x.OtvaraMin, x.ZatvaraMin })
                .FirstOrDefaultAsync();

            if (hours == null) return (false, 9 * 60, 17 * 60); // fallback
            if (hours.IsClosed) return (true, 0, 0);
            return (false, hours.OtvaraMin ?? 9 * 60, hours.ZatvaraMin ?? 17 * 60);
        }

        private static bool IsWithinWorkingHours(DateTime slot, int openMin, int closeMin)
        {
            var minutes = slot.Hour * 60 + slot.Minute;
            return minutes >= openMin && minutes < closeMin;
        }

        public async Task<bool> CancelAsync(
            int rezervacijaId,
            int? requireKorisnikId,
            int? requireZaposlenikId,
            string? razlogOtkaza)
        {
            var entity = await _context.Rezervacije.FirstOrDefaultAsync(r => r.Id == rezervacijaId);
            if (entity == null) return false;

            if (requireKorisnikId.HasValue && entity.KorisnikId != requireKorisnikId.Value)
            {
                return false;
            }

            if (requireZaposlenikId.HasValue && entity.ZaposlenikId != requireZaposlenikId.Value)
            {
                return false;
            }

            if (entity.IsPlacena)
            {
                // MVP: ne dozvoljavamo otkazivanje plaćene rezervacije.
                return false;
            }

            if (entity.IsOtkazana) return true;

            entity.IsOtkazana = true;
            entity.OtkazanaAt = DateTime.UtcNow;
            entity.RazlogOtkaza = string.IsNullOrWhiteSpace(razlogOtkaza)
                ? null
                : razlogOtkaza.Trim();
            entity.IsPotvrdjena = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RezervacijaCalendarItemDTO>> GetCalendarAsync(
            DateTime from,
            DateTime to,
            int? zaposlenikId,
            bool includeOtkazane = false,
            int? uslugaId = null,
            int? prostorijaId = null,
            string? q = null)
        {
            var start = from.Date;
            var endExclusive = to.Date.AddDays(1);

            var query = _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Where(r => r.DatumRezervacije >= start && r.DatumRezervacije < endExclusive)
                .AsQueryable();

            if (!includeOtkazane)
            {
                query = query.Where(r => !r.IsOtkazana);
            }

            if (zaposlenikId.HasValue)
            {
                query = query.Where(r => r.ZaposlenikId == zaposlenikId.Value);
            }

            if (uslugaId.HasValue)
            {
                query = query.Where(r => r.UslugaId == uslugaId.Value);
            }

            if (prostorijaId.HasValue)
            {
                if (prostorijaId.Value == 0)
                {
                    query = query.Where(r => r.ProstorijaId == null);
                }
                else
                {
                    query = query.Where(r => r.ProstorijaId == prostorijaId.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                if (t.Length > 200)
                {
                    t = t[..200];
                }

                query = query.Where(r =>
                    r.Id.ToString().Contains(t) ||
                    r.KorisnikId.ToString().Contains(t) ||
                    (r.Korisnik.Ime + " " + r.Korisnik.Prezime).Contains(t) ||
                    (r.Korisnik.Email != null && r.Korisnik.Email.Contains(t)) ||
                    (r.Korisnik.PhoneNumber != null && r.Korisnik.PhoneNumber.Contains(t)) ||
                    (r.Zaposlenik.Ime + " " + r.Zaposlenik.Prezime).Contains(t) ||
                    r.Usluga.Naziv.Contains(t) ||
                    (r.Prostorija != null && r.Prostorija.Naziv.Contains(t)) ||
                    (r.RazlogOtkaza != null && r.RazlogOtkaza.Contains(t)));
            }

            var list = await query
                .OrderBy(r => r.DatumRezervacije)
                .Select(r => new RezervacijaCalendarItemDTO
                {
                    Id = r.Id,
                    DatumRezervacije = r.DatumRezervacije,
                    IsPotvrdjena = r.IsPotvrdjena,
                    IsPlacena = r.IsPlacena,
                    IsOtkazana = r.IsOtkazana,
                    ZaposlenikId = r.ZaposlenikId,
                    ZaposlenikIme = r.Zaposlenik.Ime + " " + r.Zaposlenik.Prezime,
                    ProstorijaId = r.ProstorijaId,
                    ProstorijaNaziv = r.Prostorija != null ? r.Prostorija.Naziv : null,
                    KorisnikId = r.KorisnikId,
                    KorisnikIme = r.Korisnik.Ime + " " + r.Korisnik.Prezime,
                    KorisnikTelefon = r.Korisnik.PhoneNumber,
                    KorisnikEmail = r.Korisnik.Email,
                    UslugaId = r.UslugaId,
                    UslugaNaziv = r.Usluga.Naziv,
                    UslugaTrajanjeMinuta = r.Usluga.TrajanjeMinuta,
                    UslugaCijena = r.Usluga.Cijena,
                    RazlogOtkaza = r.RazlogOtkaza,
                })
                .ToListAsync();

            return list;
        }

        public async Task<List<RezervacijaPovijestItemDto>> GetPovijestZaKlijentaAsync(
            bool isAdmin,
            int zaposlenikIdIfTherapist,
            int korisnikKlijentId,
            int? excludeRezervacijaId,
            int take)
        {
            var takeSafe = take <= 0 ? 20 : Math.Min(take, 100);

            if (!isAdmin)
            {
                var linked = await _context.Rezervacije.AsNoTracking().AnyAsync(r =>
                    r.KorisnikId == korisnikKlijentId &&
                    r.ZaposlenikId == zaposlenikIdIfTherapist);
                if (!linked)
                    return new List<RezervacijaPovijestItemDto>();
            }

            var query = _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Where(r => r.KorisnikId == korisnikKlijentId);

            if (!isAdmin)
                query = query.Where(r => r.ZaposlenikId == zaposlenikIdIfTherapist);

            if (excludeRezervacijaId.HasValue)
                query = query.Where(r => r.Id != excludeRezervacijaId.Value);

            return await query
                .OrderByDescending(r => r.DatumRezervacije)
                .Take(takeSafe)
                .Select(r => new RezervacijaPovijestItemDto
                {
                    Id = r.Id,
                    DatumRezervacije = r.DatumRezervacije,
                    UslugaNaziv = r.Usluga.Naziv,
                    IsPotvrdjena = r.IsPotvrdjena,
                    IsPlacena = r.IsPlacena,
                    IsOtkazana = r.IsOtkazana,
                })
                .ToListAsync();
        }

        private async Task<List<RezervacijaDTO>> MapAndEnrichAsync(List<Rezervacija> list)
        {
            var dtos = _mapper.Map<List<RezervacijaDTO>>(list);
            await ApplyPremiumFlagsAsync(dtos).ConfigureAwait(false);
            return dtos;
        }

        private async Task<RezervacijaDTO> MapSingleAndEnrichAsync(Rezervacija entity)
        {
            var dto = _mapper.Map<RezervacijaDTO>(entity);
            await ApplyPremiumFlagsAsync(new List<RezervacijaDTO> { dto }).ConfigureAwait(false);
            return dto;
        }

        /// <summary>
        /// VIP heuristika: min. 3 završena plaćena termina (neotkazana) po korisniku.
        /// </summary>
        private async Task ApplyPremiumFlagsAsync(List<RezervacijaDTO> dtos)
        {
            if (dtos.Count == 0)
                return;

            var ids = dtos.Select(d => d.KorisnikId).Distinct().ToList();
            var premiumIds = await _context.Rezervacije
                .AsNoTracking()
                .Where(r => ids.Contains(r.KorisnikId) && r.IsPlacena && !r.IsOtkazana)
                .GroupBy(r => r.KorisnikId)
                .Where(g => g.Count() >= 3)
                .Select(g => g.Key)
                .ToListAsync();

            var set = premiumIds.ToHashSet();
            foreach (var d in dtos)
                d.PremiumKlijent = set.Contains(d.KorisnikId);
        }
    }
}

