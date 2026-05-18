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
    public class ZaposlenikService : BaseService<ZaposlenikDTO, Zaposlenik, object>, IZaposlenikService
    {
        private const int DefaultSpaCentarId = 1;
        private const int SeniorTherapistMinAppointments = 20;

        private static readonly string[] DayNames =
            { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        private static readonly string[] MonthNames =
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        };

        public ZaposlenikService(NuaSpaContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<IEnumerable<ZaposlenikDTO>> Get(object? search = null)
        {
            var list = await _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .OrderBy(z => z.Prezime)
                .ThenBy(z => z.Ime)
                .ToListAsync();

            return list.Select(MapToDto).ToList();
        }

        public override async Task<ZaposlenikDTO> GetById(int id)
        {
            var entity = await _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .FirstOrDefaultAsync(z => z.Id == id);

            return entity == null
                ? _mapper.Map<ZaposlenikDTO>(null!)
                : MapToDto(entity);
        }

        public override async Task<ZaposlenikDTO> Insert(ZaposlenikDTO dto)
        {
            var entity = _mapper.Map<Zaposlenik>(dto);
            entity.DatumZaposlenja = DateTime.UtcNow;
            ApplyDtoFields(entity, dto);

            _context.Zaposlenici.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity)
                .Reference(z => z.KategorijaUsluga)
                .LoadAsync();

            return MapToDto(entity);
        }

        public async Task<ZaposlenikDTO?> UpdateAsync(int id, ZaposlenikDTO dto)
        {
            var entity = await _context.Zaposlenici
                .Include(z => z.KategorijaUsluga)
                .FirstOrDefaultAsync(z => z.Id == id);
            if (entity == null) return null;

            entity.Ime = dto.Ime.Trim();
            entity.Prezime = dto.Prezime.Trim();
            entity.Specijalizacija = dto.Specijalizacija.Trim();
            ApplyDtoFields(entity, dto);

            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<string?> ValidateSpecijalizacijaAsync(int? kategorijaUslugaId, string specijalizacija)
        {
            var names = ParseSpecNames(specijalizacija);
            if (names.Count == 0)
            {
                return "Specijalizacija je obavezna.";
            }

            if (kategorijaUslugaId is not > 0)
            {
                return null;
            }

            var validNames = await _context.Usluge
                .AsNoTracking()
                .Where(u => u.KategorijaUslugaId == kategorijaUslugaId && !u.IsDeleted)
                .Select(u => u.Naziv)
                .ToListAsync();

            var validSet = validNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var invalid = names.Where(n => !validSet.Contains(n)).ToList();
            if (invalid.Count == 0)
            {
                return null;
            }

            return $"Nepoznate usluge za odabranu kategoriju: {string.Join(", ", invalid)}.";
        }

        public async Task<TherapistAdminProfileDto?> GetAdminProfileAsync(
            int zaposlenikId,
            int maxReviews = 20,
            DateTime? kpiFrom = null,
            DateTime? kpiTo = null)
        {
            var z = await _context.Zaposlenici
                .AsNoTracking()
                .Include(x => x.KategorijaUsluga)
                .FirstOrDefaultAsync(x => x.Id == zaposlenikId);
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

            var today = DateTime.UtcNow.Date;
            var toD = (kpiTo ?? today).Date;
            var fromD = (kpiFrom ?? toD.AddDays(-30)).Date;

            var kpi = await GetKpiAsync(zaposlenikId, fromD, toD);
            var weekStart = MondayOf(today);
            var schedule = await BuildWeeklyScheduleAsync(zaposlenikId, weekStart);
            var topFrom = toD.AddDays(-90);
            var topServices = await BuildTopServicesAsync(zaposlenikId, topFrom, toD.AddDays(1));

            return new TherapistAdminProfileDto
            {
                Terapeut = MapToDto(z),
                PovezanEmail = korisnik?.Email,
                ImaKorisnickiNalog = korisnik != null,
                InternaNapomena = korisnik?.NapomenaZaTerapeuta,
                NedavneRecenzije = reviews,
                LokacijaPrikaz = await ResolveLokacijaPrikazAsync(z),
                Uloga = kpi?.Uloga ?? ResolveUloga(0),
                Kpi = kpi,
                SedmicniRaspored = schedule,
                TopUsluge = topServices,
            };
        }

        public async Task<TherapistKpiDTO?> GetKpiAsync(int zaposlenikId, DateTime from, DateTime to)
        {
            var exists = await _context.Zaposlenici
                .AsNoTracking()
                .AnyAsync(z => z.Id == zaposlenikId);
            if (!exists) return null;

            var start = from.Date;
            var endExclusive = to.Date.AddDays(1);
            var periodDays = (to.Date - from.Date).Days + 1;
            var prevEnd = start;
            var prevStart = start.AddDays(-periodDays);

            var current = await ComputeKpiMetricsAsync(zaposlenikId, start, endExclusive);
            var previous = await ComputeKpiMetricsAsync(zaposlenikId, prevStart, prevEnd);

            var cancelRate = current.Ukupno == 0
                ? 0.0
                : Math.Round(current.Otkazane * 100.0 / current.Ukupno, 1);

            int? satisfaction = current.AvgRating > 0
                ? (int)Math.Round(current.AvgRating / 5.0 * 100, MidpointRounding.AwayFromZero)
                : null;

            var prevSatisfaction = previous.AvgRating > 0
                ? (int)Math.Round(previous.AvgRating / 5.0 * 100, MidpointRounding.AwayFromZero)
                : (int?)null;

            return new TherapistKpiDTO
            {
                ZaposlenikId = zaposlenikId,
                From = start,
                To = to.Date,
                UkupnoRezervacija = current.Ukupno,
                PotvrdjeneRezervacije = current.Potvrdjene,
                OtkazaneRezervacije = current.Otkazane,
                PlaceneRezervacije = current.Placene,
                Prihod = current.Prihod,
                ProsjecnaOcjena = current.AvgRating,
                StopaOtkazivanjaPostotak = cancelRate,
                ZadovoljstvoKlijenataPostotak = satisfaction,
                Uloga = ResolveUloga(current.Ukupno),
                TrendUkupnoRezervacijaPostotak = PercentTrend(current.Ukupno, previous.Ukupno),
                TrendPotvrdjenePostotak = PercentTrend(current.Potvrdjene, previous.Potvrdjene),
                TrendOtkazanePostotak = PercentTrend(current.Otkazane, previous.Otkazane),
                TrendProsjecnaOcjenaDelta = DeltaTrend(current.AvgRating, previous.AvgRating),
                TrendPrihodPostotak = PercentTrendDecimal(current.Prihod, previous.Prihod),
                TrendZadovoljstvoPostotak = satisfaction.HasValue && prevSatisfaction.HasValue
                    ? satisfaction.Value - prevSatisfaction.Value
                    : null,
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

        private static void ApplyDtoFields(Zaposlenik entity, ZaposlenikDTO dto)
        {
            entity.KategorijaUslugaId = dto.KategorijaUslugaId is > 0
                ? dto.KategorijaUslugaId
                : null;
            entity.Jezici = NormalizeOptional(dto.Jezici, 200);
            entity.Obrazovanje = NormalizeOptional(dto.Obrazovanje, 1000);
            entity.Lokacija = NormalizeOptional(dto.Lokacija, 120);
            entity.Telefon = string.IsNullOrWhiteSpace(dto.Telefon)
                ? null
                : dto.Telefon.Trim();
        }

        private static string? NormalizeOptional(string? value, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var t = value.Trim();
            return t.Length <= maxLen ? t : t[..maxLen];
        }

        private static ZaposlenikDTO MapToDto(Zaposlenik z)
        {
            var dto = new ZaposlenikDTO
            {
                Id = z.Id,
                Ime = z.Ime,
                Prezime = z.Prezime,
                Specijalizacija = z.Specijalizacija,
                Telefon = z.Telefon,
                KategorijaUslugaId = z.KategorijaUslugaId,
                KategorijaUslugaNaziv = z.KategorijaUsluga?.Naziv,
                Jezici = z.Jezici,
                Obrazovanje = z.Obrazovanje,
                Lokacija = z.Lokacija,
                DatumZaposlenja = z.DatumZaposlenja,
            };
            return dto;
        }

        private async Task<string> ResolveLokacijaPrikazAsync(Zaposlenik z)
        {
            if (!string.IsNullOrWhiteSpace(z.Lokacija))
            {
                return z.Lokacija.Trim();
            }

            var spa = await _context.SpaCentri
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == DefaultSpaCentarId);

            if (spa == null) return "NuaSpa";

            var city = string.IsNullOrWhiteSpace(spa.Adresa)
                ? "Sarajevo"
                : spa.Adresa.Trim();
            return $"{city} · {spa.Naziv}";
        }

        private async Task<List<TherapistWeeklyScheduleDayDto>> BuildWeeklyScheduleAsync(
            int zaposlenikId,
            DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            var appointments = await _context.Rezervacije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Where(r =>
                    r.ZaposlenikId == zaposlenikId
                    && !r.IsOtkazana
                    && r.DatumRezervacije >= weekStart
                    && r.DatumRezervacije < weekEnd)
                .Select(r => new DayAppointment(
                    r.DatumRezervacije,
                    r.Usluga.TrajanjeMinuta))
                .ToListAsync();

            var rows = new List<TherapistWeeklyScheduleDayDto>();
            for (var i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var hours = FormatHoursForDay(day, appointments);
                rows.Add(new TherapistWeeklyScheduleDayDto
                {
                    DanUSedmici = i + 1,
                    Label = $"{DayNames[i]} {MonthNames[day.Month - 1]} {day.Day}",
                    HoursText = hours,
                    IsWorking = hours != "Day off",
                });
            }

            return rows;
        }

        private static string FormatHoursForDay(
            DateTime day,
            IReadOnlyList<DayAppointment> items)
        {
            var dayItems = items
                .Where(e =>
                {
                    var d = e.Start;
                    return d.Year == day.Year && d.Month == day.Month && d.Day == day.Day;
                })
                .ToList();

            if (dayItems.Count == 0) return "Day off";

            var minH = 23;
            var minM = 59;
            var maxH = 0;
            var maxM = 0;

            foreach (var e in dayItems)
            {
                var t = e.Start;
                var end = t.AddMinutes(e.DurationMinutes);
                if (t.Hour < minH || (t.Hour == minH && t.Minute < minM))
                {
                    minH = t.Hour;
                    minM = t.Minute;
                }

                if (end.Hour > maxH || (end.Hour == maxH && end.Minute > maxM))
                {
                    maxH = end.Hour;
                    maxM = end.Minute;
                }
            }

            if (dayItems.Count == 1 && minH == maxH)
            {
                return $"{Fmt(minH, minM)}–{Fmt(maxH + 1, 0)}";
            }

            return $"{Fmt(minH, minM)}–{Fmt(maxH, maxM)}";
        }

        private static string Fmt(int h, int m) =>
            $"{h:00}:{m:00}";

        private async Task<List<TherapistTopServiceDto>> BuildTopServicesAsync(
            int zaposlenikId,
            DateTime periodStart,
            DateTime endExclusive)
        {
            var grouped = await (
                from r in _context.Rezervacije.AsNoTracking()
                join u in _context.Usluge.AsNoTracking() on r.UslugaId equals u.Id
                where r.ZaposlenikId == zaposlenikId
                      && !r.IsOtkazana
                      && r.DatumRezervacije >= periodStart
                      && r.DatumRezervacije < endExclusive
                group r by u.Naziv
                into g
                orderby g.Count() descending
                select new { Naziv = g.Key, Broj = g.Count() })
                .Take(8)
                .ToListAsync();

            var total = grouped.Sum(x => x.Broj);
            if (total == 0) return new List<TherapistTopServiceDto>();

            return grouped
                .Select(x => new TherapistTopServiceDto
                {
                    Naziv = x.Naziv,
                    Broj = x.Broj,
                    Postotak = Math.Round(x.Broj * 100.0 / total, 1),
                })
                .ToList();
        }

        private async Task<KpiMetrics> ComputeKpiMetricsAsync(
            int zaposlenikId,
            DateTime start,
            DateTime endExclusive)
        {
            var rezQuery = _context.Rezervacije
                .AsNoTracking()
                .Where(r =>
                    r.ZaposlenikId == zaposlenikId
                    && r.DatumRezervacije >= start
                    && r.DatumRezervacije < endExclusive);

            var ukupno = await rezQuery.CountAsync();
            var potvrdjene = await rezQuery.Where(r => r.IsPotvrdjena && !r.IsOtkazana).CountAsync();
            var otkazane = await rezQuery.Where(r => r.IsOtkazana).CountAsync();
            var placene = await rezQuery.Where(r => r.IsPlacena && !r.IsOtkazana).CountAsync();

            var prihod = await rezQuery
                .Where(r => r.IsPlacena && !r.IsOtkazana)
                .Join(
                    _context.Usluge.AsNoTracking(),
                    r => r.UslugaId,
                    u => u.Id,
                    (r, u) => u.Cijena)
                .SumAsync(x => (decimal?)x) ?? 0m;

            var ratings = await (
                from r in _context.Recenzije.AsNoTracking()
                join rez in _context.Rezervacije.AsNoTracking()
                    on new { r.UslugaId, r.KorisnikId } equals new { rez.UslugaId, rez.KorisnikId }
                where rez.ZaposlenikId == zaposlenikId
                      && rez.DatumRezervacije >= start
                      && rez.DatumRezervacije < endExclusive
                select (double?)r.Ocjena
            ).ToListAsync();

            var avg = ratings.Count == 0
                ? 0.0
                : Math.Round(ratings.Average() ?? 0.0, 2);

            return new KpiMetrics(ukupno, potvrdjene, otkazane, placene, prihod, avg);
        }

        private static string ResolveUloga(int ukupnoRezervacija) =>
            ukupnoRezervacija >= SeniorTherapistMinAppointments
                ? "Senior Therapist"
                : "Therapist";

        private static DateTime MondayOf(DateTime d)
        {
            var day = d.Date;
            var diff = ((int)day.DayOfWeek + 6) % 7;
            return day.AddDays(-diff);
        }

        private static List<string> ParseSpecNames(string raw)
        {
            return raw
                .Split(new[] { ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static double? PercentTrend(int current, int previous)
        {
            if (previous == 0)
            {
                return current == 0 ? null : 100.0;
            }

            return Math.Round((current - previous) * 100.0 / previous, 0);
        }

        private static double? PercentTrendDecimal(decimal current, decimal previous)
        {
            if (previous == 0m)
            {
                return current == 0m ? null : 100.0;
            }

            return Math.Round((double)((current - previous) * 100m / previous), 0);
        }

        private static double? DeltaTrend(double current, double previous)
        {
            if (current == 0 && previous == 0) return null;
            return Math.Round(current - previous, 2);
        }

        private sealed record KpiMetrics(
            int Ukupno,
            int Potvrdjene,
            int Otkazane,
            int Placene,
            decimal Prihod,
            double AvgRating);

        private sealed record DayAppointment(DateTime Start, int DurationMinutes);
    }
}
