using System;
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
    public class ZaposlenikService : BaseService<ZaposlenikDTO, Zaposlenik, object>, IZaposlenikService
    {
        public ZaposlenikService(NuaSpaContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<ZaposlenikDTO> Insert(ZaposlenikDTO dto)
        {
            var entity = _mapper.Map<Zaposlenik>(dto);
            entity.DatumZaposlenja = DateTime.UtcNow;

            _context.Zaposlenici.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<ZaposlenikDTO>(entity);
        }

        public async Task<TherapistAdminProfileDto?> GetAdminProfileAsync(int zaposlenikId, int maxReviews = 20)
        {
            var z = await _context.Zaposlenici.AsNoTracking().FirstOrDefaultAsync(x => x.Id == zaposlenikId);
            if (z == null) return null;

            var korisnik = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.ZaposlenikId == zaposlenikId);

            var take = Math.Clamp(maxReviews, 1, 50);

            var reviews = await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Include(r => r.Korisnik)
                .Where(rev => _context.Rezervacije.Any(rez =>
                    rez.ZaposlenikId == zaposlenikId
                    && !rez.IsOtkazana
                    && rez.KorisnikId == rev.KorisnikId
                    && rez.UslugaId == rev.UslugaId))
                .OrderByDescending(rev => rev.CreatedAt)
                .Take(take)
                .Select(rev => new TherapistReviewRowDto
                {
                    CreatedAt = rev.CreatedAt,
                    Ocjena = rev.Ocjena,
                    Komentar = rev.Komentar,
                    UslugaNaziv = rev.Usluga.Naziv,
                    KorisnikIme = rev.Korisnik.Ime + " " +
                        (string.IsNullOrEmpty(rev.Korisnik.Prezime)
                            ? ""
                            : rev.Korisnik.Prezime.Substring(0, 1) + "."),
                })
                .ToListAsync();

            return new TherapistAdminProfileDto
            {
                Terapeut = _mapper.Map<ZaposlenikDTO>(z),
                PovezanEmail = korisnik?.Email,
                ImaKorisnickiNalog = korisnik != null,
                InternaNapomena = korisnik?.NapomenaZaTerapeuta,
                NedavneRecenzije = reviews,
            };
        }

        public async Task<bool> UpdateInternaNapomenaAsync(int zaposlenikId, string? napomena)
        {
            var korisnik = await _context.Users
                .FirstOrDefaultAsync(k => k.ZaposlenikId == zaposlenikId);
            if (korisnik == null) return false;

            korisnik.NapomenaZaTerapeuta = string.IsNullOrWhiteSpace(napomena) ? null : napomena.Trim();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
