using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services
{
    public class RecenzijaService : IRecenzijaService
    {
        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public RecenzijaService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<RecenzijaDTO>> GetByUslugaAsync(
            int uslugaId,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize)
        {
            (page, pageSize) = PaginationHelper.Normalize(page, pageSize);
            var query = _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Where(r => r.UslugaId == uslugaId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt);

            var paged = await PaginationHelper.ToPagedAsync(query, page, pageSize);
            return new PagedResult<RecenzijaDTO>
            {
                Ukupno = paged.Ukupno,
                Stranica = paged.Stranica,
                VelicinaStranice = paged.VelicinaStranice,
                Items = _mapper.Map<IReadOnlyList<RecenzijaDTO>>(paged.Items),
            };
        }

        public async Task<RecenzijaDTO?> GetByIdAsync(int id)
        {
            var entity = await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Include(r => r.Zaposlenik)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            return entity == null ? null : _mapper.Map<RecenzijaDTO>(entity);
        }

        public async Task<RecenzijaDTO> CreateAsync(int korisnikId, RecenzijaCreateDTO dto)
        {
            await ValidateCreateAsync(korisnikId, dto);

            var entity = new Recenzija
            {
                KorisnikId = korisnikId,
                UslugaId = dto.UslugaId,
                ZaposlenikId = dto.ZaposlenikId,
                Ocjena = dto.Ocjena,
                Komentar = dto.Komentar.Trim(),
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.Recenzije.Add(entity);
            await _context.SaveChangesAsync();

            var created = await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .FirstAsync(r => r.Id == entity.Id);

            return _mapper.Map<RecenzijaDTO>(created);
        }

        private async Task ValidateCreateAsync(int korisnikId, RecenzijaCreateDTO dto)
        {
            if (dto.ZaposlenikId <= 0)
            {
                throw new BusinessRuleException("Terapeut je obavezan.");
            }

            if (dto.UslugaId <= 0)
            {
                throw new BusinessRuleException("Usluga je obavezna.");
            }

            if (dto.Ocjena is < 1 or > 5)
            {
                throw new BusinessRuleException("Ocjena mora biti između 1 i 5.");
            }

            if (string.IsNullOrWhiteSpace(dto.Komentar))
            {
                throw new BusinessRuleException("Komentar je obavezan.");
            }

            var zaposlenik = await _context.Zaposlenici
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == dto.ZaposlenikId);
            if (zaposlenik == null)
            {
                throw new BusinessRuleException("Terapeut ne postoji.");
            }

            if (zaposlenik.KategorijaUslugaId is not > 0)
            {
                throw new BusinessRuleException(
                    "Terapeut nema dodijeljenu kategoriju usluga.");
            }

            var usluga = await _context.Usluge
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == dto.UslugaId && !u.IsDeleted);
            if (usluga == null)
            {
                throw new BusinessRuleException("Usluga ne postoji.");
            }

            if (usluga.KategorijaUslugaId != zaposlenik.KategorijaUslugaId)
            {
                throw new BusinessRuleException(
                    "Odabrana usluga ne pripada kategoriji terapeuta.");
            }

            var hasCompletedVisit = await _context.Rezervacije.AsNoTracking().AnyAsync(r =>
                r.KorisnikId == korisnikId &&
                r.UslugaId == dto.UslugaId &&
                r.ZaposlenikId == dto.ZaposlenikId &&
                r.Status == RezervacijaStatus.Completed);

            if (!hasCompletedVisit)
            {
                throw new BusinessRuleException(
                    "Recenziju je moguće ostaviti tek nakon završenog termina.");
            }
        }

        public async Task<AdminReviewsDashboardDto> GetAdminDashboardAsync(
            DateTime from,
            DateTime toExclusive,
            int page,
            int pageSize,
            string? search,
            int? minOcjena,
            int? maxOcjena,
            int? uslugaId,
            int? zaposlenikId,
            CancellationToken cancellationToken = default)
        {
            page = Math.Clamp(page, 1, 10_000);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var span = toExclusive - from;
            if (span <= TimeSpan.Zero)
            {
                span = TimeSpan.FromDays(7);
                toExclusive = from.Add(span);
            }

            var prevTo = from;
            var prevFrom = from - span;

            var filteredCurrent = BuildAdminReviewQuery(
                from, toExclusive, search, minOcjena, maxOcjena, uslugaId, zaposlenikId);
            var filteredPrev = BuildAdminReviewQuery(
                prevFrom, prevTo, search, minOcjena, maxOcjena, uslugaId, zaposlenikId);

            var total = await filteredCurrent.CountAsync(cancellationToken);
            var prevTotal = await filteredPrev.CountAsync(cancellationToken);

            var avg = total == 0
                ? 0d
                : Math.Round(await filteredCurrent.AverageAsync(r => (double)r.Ocjena, cancellationToken), 2);

            double? prevAvg = null;
            if (prevTotal > 0)
            {
                prevAvg = Math.Round(
                    await filteredPrev.AverageAsync(r => (double)r.Ocjena, cancellationToken), 2);
            }

            var positiveCount = total == 0
                ? 0
                : await filteredCurrent.CountAsync(r => r.Ocjena >= 4, cancellationToken);
            var postPos = total == 0 ? 0 : Math.Round(100.0 * positiveCount / total, 1);

            double? prevPostPos = null;
            if (prevTotal > 0)
            {
                var prevPosCount = await filteredPrev.CountAsync(r => r.Ocjena >= 4, cancellationToken);
                prevPostPos = Math.Round(100.0 * prevPosCount / prevTotal, 1);
            }

            double? postOdg = null;
            double? postOdgPrev = null;
            if (total > 0)
            {
                var odg = await filteredCurrent.CountAsync(
                    r => r.AdminOdgovor != null && r.AdminOdgovor != string.Empty,
                    cancellationToken);
                postOdg = Math.Round(100.0 * odg / total, 1);
            }

            if (prevTotal > 0)
            {
                var odgP = await filteredPrev.CountAsync(
                    r => r.AdminOdgovor != null && r.AdminOdgovor != string.Empty,
                    cancellationToken);
                postOdgPrev = Math.Round(100.0 * odgP / prevTotal, 1);
            }

            var distList = await filteredCurrent
                .GroupBy(r => r.Ocjena)
                .Select(g => new { Star = g.Key, Cnt = g.Count() })
                .ToListAsync(cancellationToken);

            var dist = new Dictionary<int, int>();
            for (var s = 1; s <= 5; s++) dist[s] = 0;
            foreach (var x in distList)
            {
                if (x.Star is >= 1 and <= 5)
                    dist[x.Star] = x.Cnt;
            }

            var top = await filteredCurrent
                .GroupBy(r => r.Usluga.Naziv)
                .Select(g => new AdminTopUslugaOcjenaDto
                {
                    Naziv = g.Key,
                    Prosjek = Math.Round(g.Average(x => (double)x.Ocjena), 2)
                })
                .OrderByDescending(x => x.Prosjek)
                .ThenBy(x => x.Naziv)
                .Take(5)
                .ToListAsync(cancellationToken);

            AdminReviewQuoteDto? quote = null;
            if (total > 0)
            {
                quote = await filteredCurrent
                    .Where(r => !string.IsNullOrWhiteSpace(r.Komentar))
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new AdminReviewQuoteDto
                    {
                        Tekst = r.Komentar,
                        Ocjena = r.Ocjena,
                        Autor = (r.Korisnik.Ime + " " + r.Korisnik.Prezime).Trim()
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var reviewEntities = await filteredCurrent
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var rows = await MapAdminReviewRowsAsync(reviewEntities, cancellationToken);

            return new AdminReviewsDashboardDto
            {
                Ukupno = total,
                Stranica = page,
                VelicinaStranice = pageSize,
                Redovi = rows,
                ProsjecnaOcjena = avg,
                ProsjecnaOcjenaPrethodno = prevAvg,
                UkupnoPrethodno = prevTotal,
                PostotakPozitivnih = postPos,
                PostotakPozitivnihPrethodno = prevPostPos,
                PostotakOdgovora = postOdg,
                PostotakOdgovoraPrethodno = postOdgPrev,
                DistribucijaOcjena = dist,
                TopUsluge = top,
                IstaknutaRecenzija = quote
            };
        }

        public async Task<byte[]> GetAdminDashboardCsvAsync(
            DateTime from,
            DateTime toExclusive,
            string? search,
            int? minOcjena,
            int? maxOcjena,
            int? uslugaId,
            int? zaposlenikId,
            CancellationToken cancellationToken = default)
        {
            var q = BuildAdminReviewQuery(from, toExclusive, search, minOcjena, maxOcjena, uslugaId, zaposlenikId)
                .OrderByDescending(r => r.CreatedAt);

            var reviewEntities = await q
                .Take(PaginationConstants.MaxPageSize * 10)
                .ToListAsync(cancellationToken);

            var rows = await MapAdminReviewRowsAsync(reviewEntities, cancellationToken);

            var sb = new StringBuilder();
            sb.AppendLine(
                "Id,CreatedAt,Ocjena,Korisnik,BrojPosjeta,Usluga,Terapeut,Izvor,Komentar,AdminOdgovor");
            foreach (var r in rows)
            {
                sb.Append(r.Id).Append(',');
                sb.Append(r.CreatedAt.ToString("o", CultureInfo.InvariantCulture)).Append(',');
                sb.Append(r.Ocjena).Append(',');
                sb.Append(CsvEscape(r.KorisnikPunoIme)).Append(',');
                sb.Append(r.BrojPosjeta).Append(',');
                sb.Append(CsvEscape(r.UslugaNaziv)).Append(',');
                sb.Append(CsvEscape(r.TerapeutIme ?? "")).Append(',');
                sb.Append(CsvEscape(r.Izvor)).Append(',');
                sb.Append(CsvEscape(r.Komentar)).Append(',');
                sb.Append(CsvEscape(r.AdminOdgovor ?? "")).AppendLine();
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string CsvEscape(string? s)
        {
            s ??= "";
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private IQueryable<Recenzija> BuildAdminReviewQuery(
            DateTime from,
            DateTime toExclusive,
            string? search,
            int? minOcjena,
            int? maxOcjena,
            int? uslugaId,
            int? zaposlenikId)
        {
            var q = _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Where(r => !r.IsDeleted && r.CreatedAt >= from && r.CreatedAt < toExclusive);

            if (minOcjena is { } minO)
                q = q.Where(r => r.Ocjena >= minO);
            if (maxOcjena is { } maxO)
                q = q.Where(r => r.Ocjena <= maxO);
            if (uslugaId is { } uid)
                q = q.Where(r => r.UslugaId == uid);

            if (zaposlenikId is { } zid)
            {
                q = q.Where(rev => _context.Rezervacije.Any(rez =>
                    rez.ZaposlenikId == zid
                    && rez.KorisnikId == rev.KorisnikId
                    && rez.UslugaId == rev.UslugaId
                    && !rez.IsOtkazana));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = search.Trim();
                q = q.Where(r =>
                    r.Komentar.Contains(pattern)
                    || (r.AdminOdgovor != null && r.AdminOdgovor.Contains(pattern))
                    || (r.Korisnik.Ime + " " + r.Korisnik.Prezime).Contains(pattern)
                    || r.Usluga.Naziv.Contains(pattern));
            }

            return q;
        }

        private async Task<List<AdminReviewRowDto>> MapAdminReviewRowsAsync(
            List<Recenzija> reviews,
            CancellationToken ct)
        {
            if (reviews.Count == 0)
            {
                return new List<AdminReviewRowDto>();
            }

            var korisnikIds = reviews.Select(r => r.KorisnikId).Distinct().ToList();
            var visitCounts = await _context.Rezervacije.AsNoTracking()
                .Where(r => korisnikIds.Contains(r.KorisnikId) && !r.IsOtkazana && r.IsPotvrdjena)
                .GroupBy(r => r.KorisnikId)
                .Select(g => new { KorisnikId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.KorisnikId, x => x.Count, ct);

            var zaposIds = reviews
                .Where(r => r.ZaposlenikId.HasValue)
                .Select(r => r.ZaposlenikId!.Value)
                .Distinct()
                .ToList();

            var therapistById = zaposIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Zaposlenici.AsNoTracking()
                    .Where(z => zaposIds.Contains(z.Id))
                    .ToDictionaryAsync(
                        z => z.Id,
                        z => (z.Ime + " " + z.Prezime).Trim(),
                        ct);

            var needFallback = reviews.Where(r => !r.ZaposlenikId.HasValue).ToList();
            var fallbackTerapeut = new Dictionary<int, string>();
            if (needFallback.Count > 0)
            {
                var kIds = needFallback.Select(r => r.KorisnikId).Distinct().ToList();
                var rezData = await _context.Rezervacije.AsNoTracking()
                    .Where(r => !r.IsOtkazana && kIds.Contains(r.KorisnikId))
                    .Select(r => new
                    {
                        r.KorisnikId,
                        r.UslugaId,
                        r.DatumRezervacije,
                        Terapeut = r.Zaposlenik.Ime + " " + r.Zaposlenik.Prezime,
                    })
                    .ToListAsync(ct);

                foreach (var rev in needFallback)
                {
                    var match = rezData
                        .Where(r =>
                            r.KorisnikId == rev.KorisnikId
                            && r.UslugaId == rev.UslugaId
                            && r.DatumRezervacije <= rev.CreatedAt)
                        .OrderByDescending(r => r.DatumRezervacije)
                        .FirstOrDefault();

                    if (match != null)
                    {
                        fallbackTerapeut[rev.Id] = match.Terapeut.Trim();
                    }
                }
            }

            return reviews.Select(rev =>
            {
                string? terapeut = null;
                if (rev.ZaposlenikId is int zid && therapistById.TryGetValue(zid, out var direct))
                {
                    terapeut = direct;
                }
                else if (fallbackTerapeut.TryGetValue(rev.Id, out var fb))
                {
                    terapeut = fb;
                }

                visitCounts.TryGetValue(rev.KorisnikId, out var visits);

                return new AdminReviewRowDto
                {
                    Id = rev.Id,
                    CreatedAt = rev.CreatedAt,
                    Ocjena = rev.Ocjena,
                    Komentar = rev.Komentar,
                    KorisnikPunoIme = (rev.Korisnik.Ime + " " + rev.Korisnik.Prezime).Trim(),
                    BrojPosjeta = visits,
                    UslugaNaziv = rev.Usluga.Naziv,
                    TerapeutIme = terapeut,
                    Izvor = "NuaSpa",
                    AdminOdgovor = rev.AdminOdgovor,
                };
            }).ToList();
        }

        public async Task<bool> SetAdminOdgovorAsync(
            int recenzijaId,
            string? tekst,
            CancellationToken cancellationToken = default)
        {
            var entity = await _context.Recenzije
                .FirstOrDefaultAsync(r => r.Id == recenzijaId && !r.IsDeleted, cancellationToken);
            if (entity == null) return false;

            if (string.IsNullOrWhiteSpace(tekst))
                entity.AdminOdgovor = null;
            else
            {
                var t = tekst.Trim();
                entity.AdminOdgovor = t.Length > 2000 ? t[..2000] : t;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
