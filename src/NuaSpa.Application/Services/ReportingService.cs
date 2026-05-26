using NuaSpa.Application.Interfaces; // Ovo omogućava da vidi IReportingService
using NuaSpa.Application.DTOs;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using Microsoft.EntityFrameworkCore; // Obavezno za ToListAsync() i Count()
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NuaSpa.Application.Services;
public class ReportingService : IReportingService
{
    private readonly NuaSpaContext _context;

    public ReportingService(NuaSpaContext context)
    {
        _context = context;
        // QuestPDF licenca (obavezno za community verziju)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Plaćanja koja ulaze u prihod: bez rezervacije (npr. ručni unos) ili plaćena aktivna rezervacija.
    /// Usklađeno s admin finance (nije refundirano / otkazano-plaćeno).
    /// </summary>
    private IQueryable<Placanje> QueryPrihodnaPlacanja(DateTime startInclusive, DateTime endExclusive)
    {
        return _context.Placanja.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Where(p => p.DatumPlacanja >= startInclusive && p.DatumPlacanja < endExclusive)
            .Where(p =>
                p.RezervacijaId == null
                || (p.Rezervacija != null
                    && !p.Rezervacija.IsDeleted
                    && p.Rezervacija.IsPlacena
                    && !p.Rezervacija.IsOtkazana));
    }

    public async Task<byte[]> GenerateTopUslugeReport()
    {
        var data = await _context.Rezervacije
            .AsNoTracking()
            .GroupBy(r => r.UslugaId)
            .Select(g => new
            {
                UslugaId = g.Key,
                BrojRezervacija = g.Count(),
            })
            .OrderByDescending(x => x.BrojRezervacija)
            .Take(5)
            .Join(
                _context.Usluge.AsNoTracking(),
                g => g.UslugaId,
                u => u.Id,
                (g, u) => new TopUslugaDTO
                {
                    Naziv = u.Naziv,
                    BrojRezervacija = g.BrojRezervacija,
                    UkupnaZarada = g.BrojRezervacija * u.Cijena,
                })
            .ToListAsync();

        // 2. Kreiranje PDF dokumenta
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                // HEADER
                page.Header().Text("NuaSpa - Top 5 Usluga Izvještaj")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                // CONTENT
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // R.br.
                        columns.RelativeColumn();    // Naziv
                        columns.RelativeColumn();    // Broj rezervacija
                        columns.RelativeColumn();    // Zarada
                    });

                    // Naslovna traka tabele
                    table.Header(header =>
                    {
                        header.Cell().Text("#");
                        header.Cell().Text("Naziv usluge");
                        header.Cell().Text("Broj rezervacija");
                        header.Cell().Text("Ukupna zarada");
                    });

                    // Podaci
                    int i = 1;
                    foreach (var item in data)
                    {
                        table.Cell().Text(i++.ToString());
                        table.Cell().Text(item.Naziv);
                        table.Cell().Text(item.BrojRezervacija.ToString());
                        table.Cell().Text($"{item.UkupnaZarada} KM");
                    }
                });

                // FOOTER
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Stranica ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<AdminKpiDTO> GetAdminKpisAsync(DateTime date)
    {
        var day = date.Date;
        var next = day.AddDays(1);

        var ukupnoRezervacija = await _context.Rezervacije.CountAsync(r => !r.IsDeleted);
        var rezervacijeDanas = await _context.Rezervacije
            .Where(r => !r.IsDeleted)
            .Where(r => r.DatumRezervacije >= day && r.DatumRezervacije < next)
            .CountAsync();

        // Prihod iz stvarnih uplata (Placanja), ne iz cijene usluge na rezervaciji.
        var prihodDanas = await QueryPrihodnaPlacanja(day, next).SumAsync(p => p.Iznos);

        var aktivniTerapeuti = await _context.Zaposlenici.CountAsync();

        var prosjecnaOcjena = await _context.Recenzije
            .Select(r => (double?)r.Ocjena)
            .AverageAsync() ?? 0.0;

        return new AdminKpiDTO
        {
            UkupnoRezervacija = ukupnoRezervacija,
            RezervacijeDanas = rezervacijeDanas,
            PrihodDanas = prihodDanas,
            AktivniTerapeuti = aktivniTerapeuti,
            ProsjecnaOcjena = Math.Round(prosjecnaOcjena, 2),
        };
    }

    public async Task<DesktopHomeOverviewDto> GetDesktopHomeOverviewAsync(
        DateTime day,
        bool isAdmin,
        bool isZaposlenik,
        bool isKlijent,
        int currentUserId,
        int zaposlenikIdIfTherapist)
    {
        var dayStart = day.Date;
        var next = dayStart.AddDays(1);
        var registracijeOd = DateTime.Today.AddDays(-7);

        int? noviKlijenti = null;
        decimal prihod = 0m;

        if (isAdmin)
        {
            noviKlijenti = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.DatumRegistracije >= registracijeOd);

            prihod = await QueryPrihodnaPlacanja(dayStart, next).SumAsync(p => p.Iznos);
        }
        else if (isZaposlenik)
        {
            prihod = await QueryPrihodnaPlacanja(dayStart, next)
                .Where(p => p.Rezervacija != null && p.Rezervacija.ZaposlenikId == zaposlenikIdIfTherapist)
                .SumAsync(p => p.Iznos);
        }
        else if (isKlijent)
        {
            prihod = await QueryPrihodnaPlacanja(dayStart, next)
                .Where(p => p.Rezervacija != null && p.Rezervacija.KorisnikId == currentUserId)
                .SumAsync(p => p.Iznos);
        }

        return new DesktopHomeOverviewDto
        {
            NoviKlijentiZadnjih7Dana = noviKlijenti,
            ProcijenjeniPrihodZaDan = prihod,
            Valuta = "KM",
        };
    }

    public async Task<List<RevenuePointDTO>> GetRevenueSeriesAsync(DateTime from, DateTime to)
    {
        var start = from.Date;
        var endExclusive = to.Date.AddDays(1);

        var fromDb = await QueryPrihodnaPlacanja(start, endExclusive)
            .GroupBy(p => p.DatumPlacanja.Date)
            .Select(g => new
            {
                Datum = g.Key,
                BrojPlacanja = g.Count(),
                Prihod = g.Sum(x => x.Iznos),
            })
            .ToListAsync();

        var map = fromDb.ToDictionary(x => x.Datum, x => (x.BrojPlacanja, x.Prihod));
        var list = new List<RevenuePointDTO>();
        for (var d = start; d < endExclusive; d = d.AddDays(1))
        {
            if (map.TryGetValue(d, out var row))
            {
                list.Add(new RevenuePointDTO
                {
                    Datum = d,
                    BrojRezervacija = row.BrojPlacanja,
                    Prihod = row.Prihod,
                });
            }
            else
            {
                list.Add(new RevenuePointDTO
                {
                    Datum = d,
                    BrojRezervacija = 0,
                    Prihod = 0m,
                });
            }
        }

        return list;
    }

    public async Task<List<ServicePopularityDTO>> GetServicePopularityAsync(DateTime from, DateTime to, int take)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var safeTake = take <= 0 ? 8 : Math.Min(take, 30);

        var grouped = await QueryPrihodnaPlacanja(start, end)
            .Where(p => p.RezervacijaId != null && p.Rezervacija != null)
            .GroupBy(p => p.Rezervacija!.UslugaId)
            .Select(g => new
            {
                UslugaId = g.Key,
                BrojPlacanja = g.Count(),
                Prihod = g.Sum(x => x.Iznos),
            })
            .OrderByDescending(x => x.Prihod)
            .ThenByDescending(x => x.BrojPlacanja)
            .Take(safeTake)
            .ToListAsync();

        if (grouped.Count == 0)
            return new List<ServicePopularityDTO>();

        var ids = grouped.Select(x => x.UslugaId).Distinct().ToList();
        var names = await _context.Usluge.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.Naziv })
            .ToListAsync();
        var nameMap = names.ToDictionary(x => x.Id, x => x.Naziv);

        return grouped.Select(x => new ServicePopularityDTO
        {
            UslugaId = x.UslugaId,
            Naziv = nameMap.TryGetValue(x.UslugaId, out var n) ? n : "—",
            BrojRezervacija = x.BrojPlacanja,
            Prihod = x.Prihod,
        }).ToList();
    }

    public async Task<List<TopSpenderDTO>> GetTopSpendersAsync(DateTime from, DateTime to, int take)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var safeTake = take <= 0 ? 10 : Math.Min(take, 50);

        var query = QueryPrihodnaPlacanja(start, end)
            .Where(p => p.RezervacijaId != null && p.Rezervacija != null)
            .GroupBy(p => p.Rezervacija!.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                BrojPosjeta = g.Count(),
                Ukupno = g.Sum(x => x.Iznos),
                Zadnja = g.Max(x => x.DatumPlacanja),
            })
            .OrderByDescending(x => x.Ukupno)
            .ThenByDescending(x => x.BrojPosjeta)
            .Take(safeTake);

        var top = await query.ToListAsync();
        var ids = top.Select(x => x.KorisnikId).ToList();
        var users = await _context.Users
            .Where(k => ids.Contains(k.Id))
            .Select(k => new { k.Id, k.Ime, k.Prezime, k.Email })
            .ToListAsync();

        var map = users.ToDictionary(x => x.Id, x => x);
        return top.Select(x =>
        {
            map.TryGetValue(x.KorisnikId, out var u);
            return new TopSpenderDTO
            {
                KorisnikId = x.KorisnikId,
                ImePrezime = u == null ? $"#{x.KorisnikId}" : $"{u.Ime} {u.Prezime}",
                Email = u?.Email,
                BrojPosjeta = x.BrojPosjeta,
                UkupnoPotroseno = x.Ukupno,
                ZadnjaPosjeta = x.Zadnja,
            };
        }).ToList();
    }

    public async Task<List<ActivityFeedItemDto>> GetActivityFeedAsync(
        DateTime day,
        int take,
        CancellationToken cancellationToken = default)
    {
        var start = day.Date;
        var end = start.AddDays(1);
        take = Math.Clamp(take, 1, 50);
        var list = new List<ActivityFeedItemDto>();

        var rezervacije = await _context.Rezervacije.AsNoTracking()
            .Where(r => !r.IsDeleted && r.DatumRezervacije >= start && r.DatumRezervacije < end)
            .Include(r => r.Usluga)
            .Include(r => r.Korisnik)
            .OrderByDescending(r => r.DatumRezervacije)
            .Take(30)
            .ToListAsync(cancellationToken);

        foreach (var r in rezervacije)
        {
            var svc = r.Usluga?.Naziv ?? "—";
            var guest = r.Korisnik != null
                ? $"{r.Korisnik.Ime} {r.Korisnik.Prezime}".Trim()
                : "—";
            string naslov;
            if (r.IsOtkazana)
                naslov = $"Cancelled · {svc}";
            else if (r.IsPotvrdjena)
                naslov = $"Confirmed · {svc}";
            else
                naslov = $"Pending · {svc}";

            var at = r.IsOtkazana && r.OtkazanaAt.HasValue
                ? r.OtkazanaAt!.Value
                : r.DatumRezervacije;

            list.Add(new ActivityFeedItemDto
            {
                Tip = "booking",
                Naslov = naslov,
                Podnaslov = string.IsNullOrWhiteSpace(guest) ? null : guest,
                DatumVrijeme = at,
            });
        }

        var placanja = await QueryPrihodnaPlacanja(start, end)
            .Include(p => p.Rezervacija!)
            .ThenInclude(x => x.Usluga)
            .OrderByDescending(p => p.DatumPlacanja)
            .Take(25)
            .ToListAsync(cancellationToken);

        foreach (var p in placanja)
        {
            var svc = p.Rezervacija?.Usluga?.Naziv;
            list.Add(new ActivityFeedItemDto
            {
                Tip = "payment",
                Naslov = $"Payment · {p.Iznos:0.##} KM",
                Podnaslov = string.IsNullOrWhiteSpace(svc) ? "—" : svc,
                DatumVrijeme = p.DatumPlacanja,
            });
        }

        var recenzije = await _context.Recenzije.AsNoTracking()
            .Where(x => !x.IsDeleted && x.CreatedAt >= start && x.CreatedAt < end)
            .Include(x => x.Usluga)
            .Include(x => x.Korisnik)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var x in recenzije)
        {
            var guest = x.Korisnik != null
                ? $"{x.Korisnik.Ime} {x.Korisnik.Prezime}".Trim()
                : "—";
            list.Add(new ActivityFeedItemDto
            {
                Tip = "review",
                Naslov = $"Review · {x.Ocjena}★ · {x.Usluga?.Naziv ?? "—"}",
                Podnaslov = string.IsNullOrWhiteSpace(guest) ? null : guest,
                DatumVrijeme = x.CreatedAt,
            });
        }

        var klijentRole = await _context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == "KLIJENT", cancellationToken);
        if (klijentRole != null)
        {
            var clientIds = await _context.UserRoles.AsNoTracking()
                .Where(ur => ur.RoleId == klijentRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);
            var idSet = clientIds.Count == 0 ? null : clientIds.ToHashSet();
            if (idSet != null)
            {
                var newUsers = await _context.Users.AsNoTracking()
                    .Where(u =>
                        u.DatumRegistracije >= start
                        && u.DatumRegistracije < end
                        && idSet.Contains(u.Id))
                    .OrderByDescending(u => u.DatumRegistracije)
                    .Take(15)
                    .ToListAsync(cancellationToken);

                foreach (var u in newUsers)
                {
                    list.Add(new ActivityFeedItemDto
                    {
                        Tip = "client",
                        Naslov = $"New client · {u.Ime} {u.Prezime}".Trim(),
                        Podnaslov = u.Email,
                        DatumVrijeme = u.DatumRegistracije,
                    });
                }
            }
        }

        return list
            .OrderByDescending(x => x.DatumVrijeme)
            .Take(take)
            .ToList();
    }
}