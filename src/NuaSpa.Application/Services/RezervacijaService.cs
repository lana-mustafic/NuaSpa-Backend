using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

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

        public async Task<RezervacijaDTO?> GetByIdAsync(int rezervacijaId)
        {
            var entity = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .Include(r => r.Prostorija)
                .Include(r => r.RezervacijaOprema)
                .FirstOrDefaultAsync(r => r.Id == rezervacijaId);

            if (entity == null)
            {
                return null;
            }

            return await MapSingleAndEnrichAsync(entity);
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

        public async Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto, bool isAdminBooking = false)
        {
            // Validate working hours (SpaCentarId = 1)
            var hours = await GetWorkingHoursAsync(dto.DatumRezervacije);
            if (hours.IsClosed)
                throw new BusinessRuleException("Spa centar je zatvoren za odabrani dan.");

            if (!IsWithinWorkingHours(dto.DatumRezervacije, hours.OpenMin, hours.CloseMin))
                throw new BusinessRuleException("Termin je van radnog vremena spa centra.");

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
                    throw new BusinessRuleException("Prostorija nije dostupna.");

                var roomTaken = await _context.Rezervacije
                    .AsNoTracking()
                    .AnyAsync(r =>
                        !r.IsOtkazana &&
                        r.ProstorijaId == dto.ProstorijaId.Value &&
                        r.DatumRezervacije == dto.DatumRezervacije);
                if (roomTaken)
                    throw new BusinessRuleException("Prostorija je već zauzeta za odabrani termin.");
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
                    throw new BusinessRuleException("Dio opreme nije dostupan.");

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
                        throw new BusinessRuleException("Nema dovoljno opreme za odabrani termin.");
                }
            }

            await ValidateTherapistAndSlotAsync(
                dto.ZaposlenikId,
                dto.UslugaId,
                dto.DatumRezervacije);

            var entity = new Rezervacija
            {
                KorisnikId = korisnikId,
                UslugaId = dto.UslugaId,
                ZaposlenikId = dto.ZaposlenikId,
                DatumRezervacije = dto.DatumRezervacije,
                IsPotvrdjena = false,
                IsPlacena = false,
                ProstorijaId = dto.ProstorijaId,
                IsVip = isAdminBooking && dto.IsVip
            };

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
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

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
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
            if (entity.IsOtkazana) throw new BusinessRuleException("Otkazana rezervacija se ne može mijenjati.");
            if (entity.IsPlacena) throw new BusinessRuleException("Plaćena rezervacija se ne može mijenjati.");

            // Validate working hours
            var hours = await GetWorkingHoursAsync(dto.DatumRezervacije);
            if (hours.IsClosed) throw new BusinessRuleException("Spa centar je zatvoren za odabrani dan.");
            if (!IsWithinWorkingHours(dto.DatumRezervacije, hours.OpenMin, hours.CloseMin))
                throw new BusinessRuleException("Termin je van radnog vremena spa centra.");

            await ValidateTherapistAndSlotAsync(
                dto.ZaposlenikId,
                dto.UslugaId,
                dto.DatumRezervacije,
                rezervacijaId);

            // Validate room (exclude current reservation)
            if (dto.ProstorijaId.HasValue)
            {
                var prostorija = await _context.Prostorije
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.ProstorijaId.Value &&
                        x.IsAktivna &&
                        x.SpaCentarId == DefaultSpaCentarId);
                if (prostorija == null) throw new BusinessRuleException("Prostorija nije dostupna.");

                var roomTaken = await _context.Rezervacije
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.Id != rezervacijaId &&
                        !r.IsOtkazana &&
                        r.ProstorijaId == dto.ProstorijaId.Value &&
                        r.DatumRezervacije == dto.DatumRezervacije);
                if (roomTaken) throw new BusinessRuleException("Prostorija je već zauzeta za odabrani termin.");
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
                    throw new BusinessRuleException("Dio opreme nije dostupan.");

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
                        throw new BusinessRuleException("Nema dovoljno opreme za odabrani termin.");
                }
            }

            // Apply changes
            entity.DatumRezervacije = dto.DatumRezervacije;
            entity.ZaposlenikId = dto.ZaposlenikId;
            entity.UslugaId = dto.UslugaId;
            entity.ProstorijaId = dto.ProstorijaId;
            entity.IsVip = dto.IsVip;

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

        public async Task<bool> SetIsVipAsync(int rezervacijaId, bool isVip)
        {
            var entity = await _context.Rezervacije.FirstOrDefaultAsync(r => r.Id == rezervacijaId);
            if (entity == null) return false;
            if (entity.IsOtkazana) return false;

            entity.IsVip = isVip;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DateTime>> GetAvailableSlotsAsync(
            int zaposlenikId,
            DateTime date,
            int? uslugaId = null,
            int slotMinutes = 60)
        {
            var therapist = await _context.Zaposlenici.AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == zaposlenikId);
            if (therapist == null || therapist.Status != ZaposlenikStatus.Active)
            {
                return new List<DateTime>();
            }

            if (uslugaId is > 0)
            {
                slotMinutes = await GetServiceDurationMinutesAsync(uslugaId.Value);
            }

            var day = date.Date;
            var hours = await GetWorkingHoursAsync(day);
            if (hours.IsClosed) return new List<DateTime>();

            var start = day.AddMinutes(hours.OpenMin);
            var end = day.AddMinutes(hours.CloseMin);

            var taken = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Where(r =>
                    r.ZaposlenikId == zaposlenikId &&
                    !r.IsOtkazana &&
                    r.DatumRezervacije.Date == day)
                .Select(r => new { r.DatumRezervacije, r.Usluga.TrajanjeMinuta })
                .ToListAsync();

            var slots = new List<DateTime>();
            for (var t = start; t.AddMinutes(slotMinutes) <= end; t = t.AddMinutes(slotMinutes))
            {
                var slotEnd = t.AddMinutes(slotMinutes);
                var overlaps = taken.Any(b =>
                {
                    var duration = b.TrajanjeMinuta > 0 ? b.TrajanjeMinuta : 60;
                    var bEnd = b.DatumRezervacije.AddMinutes(duration);
                    return b.DatumRezervacije < slotEnd && bEnd > t;
                });
                if (!overlaps)
                {
                    slots.Add(t);
                }
            }

            return slots;
        }

        private async Task<int> GetServiceDurationMinutesAsync(int uslugaId)
        {
            var minutes = await _context.Usluge.AsNoTracking()
                .Where(u => u.Id == uslugaId)
                .Select(u => u.TrajanjeMinuta)
                .FirstOrDefaultAsync();
            return minutes > 0 ? minutes : 60;
        }

        private async Task ValidateTherapistAndSlotAsync(
            int zaposlenikId,
            int uslugaId,
            DateTime start,
            int? excludeRezervacijaId = null)
        {
            var therapist = await _context.Zaposlenici.AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == zaposlenikId);
            if (therapist == null)
            {
                throw new BusinessRuleException("Terapeut nije pronađen.");
            }

            if (therapist.Status != ZaposlenikStatus.Active)
            {
                throw new BusinessRuleException("Terapeut nije dostupan za rezervacije.");
            }

            var duration = await GetServiceDurationMinutesAsync(uslugaId);
            var end = start.AddMinutes(duration);

            var conflicts = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Where(r =>
                    r.ZaposlenikId == zaposlenikId
                    && !r.IsOtkazana
                    && (!excludeRezervacijaId.HasValue || r.Id != excludeRezervacijaId.Value)
                    && r.DatumRezervacije < end)
                .Select(r => new { r.DatumRezervacije, r.Usluga.TrajanjeMinuta })
                .ToListAsync();

            foreach (var c in conflicts)
            {
                var cDuration = c.TrajanjeMinuta > 0 ? c.TrajanjeMinuta : 60;
                var cEnd = c.DatumRezervacije.AddMinutes(cDuration);
                if (c.DatumRezervacije < end && cEnd > start)
                {
                    throw new BusinessRuleException(
                        "Terapeut je već zauzet u odabranom terminu.");
                }
            }
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

        public async Task<(bool Ok, string? Message)> DeleteAdminAsync(int rezervacijaId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.Rezervacije
                    .Include(r => r.RezervacijaOprema)
                    .FirstOrDefaultAsync(r => r.Id == rezervacijaId);

                if (entity == null)
                {
                    return (false, "Rezervacija ne postoji.");
                }

                if (entity.IsPlacena)
                {
                    return (false, "Plaćena rezervacija se ne može obrisati.");
                }

                var placanja = await _context.Placanja
                    .Where(p => p.RezervacijaId == rezervacijaId)
                    .ToListAsync();

                foreach (var p in placanja)
                {
                    p.RezervacijaId = null;
                }

                if (entity.RezervacijaOprema.Count > 0)
                {
                    _context.RezervacijeOprema.RemoveRange(entity.RezervacijaOprema);
                }

                _context.Rezervacije.Remove(entity);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, null);
            }
            catch
            {
                await tx.RollbackAsync();
                return (false, "Brisanje nije uspjelo.");
            }
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

                var tl = t.ToLowerInvariant();
                query = query.Where(r =>
                    r.Id.ToString().Contains(t) ||
                    r.KorisnikId.ToString().Contains(t) ||
                    (r.Korisnik.Ime + " " + r.Korisnik.Prezime).ToLower().Contains(tl) ||
                    (r.Korisnik.Email != null && r.Korisnik.Email.ToLower().Contains(tl)) ||
                    (r.Korisnik.PhoneNumber != null && r.Korisnik.PhoneNumber.ToLower().Contains(tl)) ||
                    (r.Zaposlenik.Ime + " " + r.Zaposlenik.Prezime).ToLower().Contains(tl) ||
                    (r.Zaposlenik.Telefon != null && r.Zaposlenik.Telefon.ToLower().Contains(tl)) ||
                    r.Usluga.Naziv.ToLower().Contains(tl) ||
                    (r.Prostorija != null && r.Prostorija.Naziv.ToLower().Contains(tl)) ||
                    (r.RazlogOtkaza != null && r.RazlogOtkaza.ToLower().Contains(tl)));
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
                    IsVip = r.IsVip,
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

