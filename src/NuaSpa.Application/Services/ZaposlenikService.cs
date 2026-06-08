using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;
using NuaSpa.Application.Services.Booking;

namespace NuaSpa.Application.Services
{
    public class ZaposlenikService : BaseService<ZaposlenikDTO, Zaposlenik, ZaposlenikSearchObject>, IZaposlenikService
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

        public override async Task<PagedResult<ZaposlenikDTO>> Get(ZaposlenikSearchObject? search = null)
        {
            var (page, pageSize) = PaginationHelper.FromSearch(search);
            var query = _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search?.Ime))
            {
                var ime = search.Ime.Trim();
                query = query.Where(z => z.Ime.Contains(ime));
            }

            if (!string.IsNullOrWhiteSpace(search?.Prezime))
            {
                var prezime = search.Prezime.Trim();
                query = query.Where(z => z.Prezime.Contains(prezime));
            }

            if (!string.IsNullOrWhiteSpace(search?.Q))
            {
                var q = search.Q.Trim().ToLower();
                query = query.Where(z =>
                    z.Ime.ToLower().Contains(q) ||
                    z.Prezime.ToLower().Contains(q) ||
                    z.Specijalizacija.ToLower().Contains(q));
            }

            if (search?.KategorijaUslugaId is int katId)
            {
                query = query.Where(z => z.KategorijaUslugaId == katId);
            }

            if (search?.Status is ZaposlenikStatus status)
            {
                query = query.Where(z => z.Status == status);
            }

            var paged = await PaginationHelper.ToPagedAsync(
                query.OrderBy(z => z.Prezime).ThenBy(z => z.Ime),
                page,
                pageSize);

            return new PagedResult<ZaposlenikDTO>
            {
                Ukupno = paged.Ukupno,
                Stranica = paged.Stranica,
                VelicinaStranice = paged.VelicinaStranice,
                Items = paged.Items.Select(MapToDto).ToList(),
            };
        }

        public override async Task<ZaposlenikDTO> GetById(int id)
        {
            var entity = await _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (entity == null)
            {
                throw new NotFoundException($"Zaposlenik id={id} ne postoji.");
            }

            return MapToDto(entity);
        }

        public override async Task<ZaposlenikDTO> Insert(ZaposlenikDTO dto)
        {
            if (dto.KategorijaUslugaId is > 0)
            {
                var katOk = await _context.KategorijeUsluga.AsNoTracking()
                    .AnyAsync(k => k.Id == dto.KategorijaUslugaId);
                if (!katOk)
                {
                    throw new BusinessRuleException("KategorijaUslugaId ne postoji.");
                }
            }

            var specError = await ValidateSpecijalizacijaAsync(
                dto.KategorijaUslugaId,
                dto.Specijalizacija);
            if (specError != null)
            {
                throw new BusinessRuleException(specError);
            }

            var entity = new Zaposlenik
            {
                Ime = dto.Ime.Trim(),
                Prezime = dto.Prezime.Trim(),
                Specijalizacija = dto.Specijalizacija.Trim(),
                DatumZaposlenja = DateTime.UtcNow,
            };
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

            if (dto.KategorijaUslugaId is > 0)
            {
                var katOk = await _context.KategorijeUsluga.AsNoTracking()
                    .AnyAsync(k => k.Id == dto.KategorijaUslugaId);
                if (!katOk)
                {
                    throw new BusinessRuleException("KategorijaUslugaId ne postoji.");
                }
            }

            var specError = await ValidateSpecijalizacijaAsync(
                dto.KategorijaUslugaId,
                dto.Specijalizacija);
            if (specError != null)
            {
                throw new BusinessRuleException(specError);
            }

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
            DateTime? kpiTo = null,
            DateTime? weekStart = null)
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
                .Where(rev => !rev.IsDeleted)
                .ForTherapist(_context, zaposlenikId)
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
                    AdminOdgovor = rev.AdminOdgovor,
                })
                .ToListAsync();

            var today = DateTime.Now.Date;
            var toD = (kpiTo ?? today).Date;
            var fromD = (kpiFrom ?? toD.AddDays(-30)).Date;

            var kpi = await GetKpiAsync(zaposlenikId, fromD, toD);
            var week = MondayOf((weekStart ?? today).Date);
            var schedule = await BuildWeeklyScheduleAsync(zaposlenikId, week);
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
                BrojRecenzija = current.ReviewCount,
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
            entity.Email = NormalizeOptional(dto.Email, 120);
            entity.Status = dto.Status;
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
                Email = z.Email,
                KategorijaUslugaId = z.KategorijaUslugaId,
                KategorijaUslugaNaziv = z.KategorijaUsluga?.Naziv,
                Jezici = z.Jezici,
                Obrazovanje = z.Obrazovanje,
                Lokacija = z.Lokacija,
                DatumZaposlenja = z.DatumZaposlenja,
                Status = z.Status,
            };
            return dto;
        }

        public async Task<IEnumerable<ZaposlenikDTO>> GetForServiceAsync(int uslugaId, bool bookableOnly = true)
        {
            var usluga = await _context.Usluge
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == uslugaId && !u.IsDeleted);
            if (usluga == null) return Array.Empty<ZaposlenikDTO>();

            var query = _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .Where(z => z.KategorijaUslugaId == usluga.KategorijaUslugaId);

            if (bookableOnly)
            {
                query = query.Where(z => z.Status == ZaposlenikStatus.Active);
            }

            var list = await query
                .OrderBy(z => z.Prezime)
                .ThenBy(z => z.Ime)
                .ToListAsync();

            return list
                .Where(z => TherapistServiceEligibility.Matches(usluga, z, bookableOnly))
                .Select(MapToDto)
                .ToList();
        }

        public async Task<bool> IsEligibleForServiceAsync(
            int zaposlenikId,
            int uslugaId,
            bool requireActive = true)
        {
            var usluga = await _context.Usluge
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == uslugaId && !u.IsDeleted);
            if (usluga == null) return false;

            var therapist = await _context.Zaposlenici
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == zaposlenikId);
            if (therapist == null) return false;

            return TherapistServiceEligibility.Matches(usluga, therapist, requireActive);
        }

        public async Task<IEnumerable<ZaposlenikDTO>> GetForCategoryAsync(
            int kategorijaUslugaId,
            bool bookableOnly = true)
        {
            if (kategorijaUslugaId <= 0)
            {
                return Array.Empty<ZaposlenikDTO>();
            }

            var query = _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .Where(z => z.KategorijaUslugaId == kategorijaUslugaId);

            if (bookableOnly)
            {
                query = query.Where(z => z.Status == ZaposlenikStatus.Active);
            }

            var list = await query
                .OrderBy(z => z.Prezime)
                .ThenBy(z => z.Ime)
                .ToListAsync();

            return list.Select(MapToDto).ToList();
        }

        public async Task<ZaposlenikDTO?> GetMeAsync(int zaposlenikId)
        {
            var entity = await _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .FirstOrDefaultAsync(z => z.Id == zaposlenikId);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<TherapistAdminRosterDto> GetAdminRosterAsync(
            DateTime? kpiFrom = null,
            DateTime? kpiTo = null,
            DateTime? weekStart = null)
        {
            var today = DateTime.UtcNow.Date;
            var toD = (kpiTo ?? today).Date;
            var fromD = (kpiFrom ?? toD.AddDays(-30)).Date;
            var week = MondayOf((weekStart ?? today).Date);
            var weekEnd = week.AddDays(7);

            var therapists = await _context.Zaposlenici
                .AsNoTracking()
                .Include(z => z.KategorijaUsluga)
                .OrderBy(z => z.Prezime)
                .ThenBy(z => z.Ime)
                .ToListAsync();

            var bookingCounts = await _context.Rezervacije
                .AsNoTracking()
                .Where(r =>
                    !r.IsOtkazana
                    && r.DatumRezervacije >= week
                    && r.DatumRezervacije < weekEnd)
                .GroupBy(r => new
                {
                    r.ZaposlenikId,
                    Day = r.DatumRezervacije.Date,
                })
                .Select(g => new
                {
                    g.Key.ZaposlenikId,
                    g.Key.Day,
                    Count = g.Count(),
                })
                .ToListAsync();

            var countLookup = bookingCounts.ToDictionary(
                x => (x.ZaposlenikId, x.Day),
                x => x.Count);

            var rows = new List<TherapistRosterRowDto>();
            foreach (var therapist in therapists)
            {
                var kpi = await GetKpiAsync(therapist.Id, fromD, toD);
                var weekDays = new List<TherapistRosterDayDto>();
                for (var i = 0; i < 7; i++)
                {
                    var day = week.AddDays(i);
                    var count = countLookup.GetValueOrDefault((therapist.Id, day));
                    weekDays.Add(new TherapistRosterDayDto
                    {
                        Date = day,
                        AppointmentCount = count,
                        Load = ResolveWeekLoad(count),
                    });
                }

                rows.Add(new TherapistRosterRowDto
                {
                    Terapeut = MapToDto(therapist),
                    ProsjecnaOcjena = kpi?.ProsjecnaOcjena ?? 0,
                    UkupnoRezervacija = kpi?.UkupnoRezervacija ?? 0,
                    BrojRecenzija = kpi?.BrojRecenzija ?? 0,
                    Uloga = kpi?.Uloga ?? ResolveUloga(0),
                    WeekDays = weekDays,
                });
            }

            return new TherapistAdminRosterDto
            {
                WeekStart = week,
                WeekEnd = weekEnd.AddDays(-1),
                KpiFrom = fromD,
                KpiTo = toD,
                Therapists = rows,
            };
        }

        private static string ResolveWeekLoad(int appointmentCount) =>
            appointmentCount switch
            {
                0 => "off",
                >= 5 => "heavy",
                >= 3 => "moderate",
                _ => "light",
            };

        public async Task<ZaposlenikDTO?> UpdateMeAsync(int zaposlenikId, TherapistSelfProfileUpdateDto dto)
        {
            var entity = await _context.Zaposlenici
                .Include(z => z.KategorijaUsluga)
                .FirstOrDefaultAsync(z => z.Id == zaposlenikId);
            if (entity == null) return null;

            entity.Telefon = string.IsNullOrWhiteSpace(dto.Telefon)
                ? null
                : dto.Telefon.Trim();
            entity.Jezici = NormalizeOptional(dto.Jezici, 200);

            await _context.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<IReadOnlyList<TherapistReviewRowDto>> GetMyReviewsAsync(
            int zaposlenikId,
            int maxReviews = 30)
        {
            var take = Math.Clamp(maxReviews, 1, 50);
            return await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Usluga)
                .Include(r => r.Korisnik)
                .Where(rev => !rev.IsDeleted)
                .ForTherapist(_context, zaposlenikId)
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
                    AdminOdgovor = rev.AdminOdgovor,
                })
                .ToListAsync();
        }

        public async Task<TherapistDashboardDto?> GetDashboardAsync(int zaposlenikId, DateTime? day = null)
        {
            var exists = await _context.Zaposlenici.AsNoTracking()
                .AnyAsync(z => z.Id == zaposlenikId);
            if (!exists) return null;

            var d = (day ?? DateTime.UtcNow).Date;
            var monthStart = new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var baseQuery = _context.Rezervacije.AsNoTracking()
                .Where(r => r.ZaposlenikId == zaposlenikId && !r.IsOtkazana);

            var todayCount = await baseQuery.CountAsync(r => r.DatumRezervacije.Date == d);
            var upcomingCount = await baseQuery.CountAsync(r => r.DatumRezervacije.Date > d);
            var completedMonth = await baseQuery.CountAsync(r =>
                r.DatumRezervacije >= monthStart
                && r.DatumRezervacije < monthEnd
                && r.IsPotvrdjena);

            var revenueMonth = await _context.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .Where(p => p.DatumPlacanja >= monthStart && p.DatumPlacanja < monthEnd)
                .Where(p =>
                    p.Rezervacija != null
                    && !p.Rezervacija.IsDeleted
                    && p.Rezervacija.ZaposlenikId == zaposlenikId)
                .SumAsync(p => (decimal?)(p.NaplaceniIznos ?? p.Iznos)) ?? 0m;

            var reviews = await _context.Recenzije
                .AsNoTracking()
                .Where(rev =>
                    !rev.IsDeleted
                    && rev.CreatedAt >= monthStart
                    && rev.CreatedAt < monthEnd)
                .ForTherapist(_context, zaposlenikId)
                .Select(rev => rev.Ocjena)
                .ToListAsync();

            return new TherapistDashboardDto
            {
                TodayAppointments = todayCount,
                UpcomingAppointments = upcomingCount,
                CompletedThisMonth = completedMonth,
                ProsjecnaOcjena = reviews.Count == 0
                    ? 0
                    : Math.Round(reviews.Average(x => (double)x), 2),
                ReviewCount = reviews.Count,
                RevenueThisMonth = revenueMonth,
            };
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

            var prihod = await _context.Placanja.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Where(p => p.Status == PlacanjeStatus.Completed)
                .Where(p => p.DatumPlacanja >= start && p.DatumPlacanja < endExclusive)
                .Where(p =>
                    p.Rezervacija != null
                    && !p.Rezervacija.IsDeleted
                    && p.Rezervacija.ZaposlenikId == zaposlenikId)
                .SumAsync(p => (decimal?)(p.NaplaceniIznos ?? p.Iznos)) ?? 0m;

            var ratings = await _context.Recenzije
                .AsNoTracking()
                .Where(r =>
                    !r.IsDeleted
                    && r.CreatedAt >= start
                    && r.CreatedAt < endExclusive)
                .ForTherapist(_context, zaposlenikId)
                .Select(r => (double?)r.Ocjena)
                .ToListAsync();

            var avg = ratings.Count == 0
                ? 0.0
                : Math.Round(ratings.Average() ?? 0.0, 2);

            var reviewCount = ratings.Count;

            return new KpiMetrics(ukupno, potvrdjene, otkazane, placene, prihod, avg, reviewCount);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Zaposlenici.FirstOrDefaultAsync(z => z.Id == id);
            if (entity == null)
            {
                throw new NotFoundException($"Zaposlenik id={id} ne postoji.");
            }

            var hasReservations = await _context.Rezervacije
                .AsNoTracking()
                .AnyAsync(r => r.ZaposlenikId == id);
            if (hasReservations)
            {
                throw new ConflictException("Terapeut ima rezervacije i ne može biti obrisan.");
            }

            try
            {
                // Otpovezuje korisnike od terapeuta (zadrži korisnike).
                await _context.Users
                    .Where(k => k.ZaposlenikId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        k => k.ZaposlenikId,
                        (int?)null));

                _context.Zaposlenici.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException(
                    "Terapeut se ne može obrisati zbog povezanih podataka u bazi.");
            }
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

        private static List<string> ParseSpecNames(string raw) =>
            TherapistServiceEligibility.ParseSpecNames(raw);

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
            double AvgRating,
            int ReviewCount);

        private sealed record DayAppointment(DateTime Start, int DurationMinutes);
    }
}
