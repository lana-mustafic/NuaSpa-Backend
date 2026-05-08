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
            bool includeOtkazane = false)
        {
            var query = _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .AsQueryable();

            if (!includeOtkazane)
            {
                query = query.Where(r => !r.IsOtkazana);
            }

            if (korisnikId is int id)
            {
                query = query.Where(r => r.KorisnikId == id);
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

            return _mapper.Map<IEnumerable<RezervacijaDTO>>(list);
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

            return _mapper.Map<IEnumerable<RezervacijaDTO>>(list);
        }

        public async Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto)
        {
            var entity = new Rezervacija
            {
                KorisnikId = korisnikId,
                UslugaId = dto.UslugaId,
                ZaposlenikId = dto.ZaposlenikId,
                DatumRezervacije = dto.DatumRezervacije,
                IsPotvrdjena = false,
                IsPlacena = false
            };

            _context.Rezervacije.Add(entity);
            await _context.SaveChangesAsync();

            // Dohvati relacije za DTO mapiranje (Korisnik/Usluga/Zaposlenik).
            var created = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .FirstAsync(r => r.Id == entity.Id);

            return _mapper.Map<RezervacijaDTO>(created);
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
            var start = day.AddHours(9);
            var end = day.AddHours(17);

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
    }
}

