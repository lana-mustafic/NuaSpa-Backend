using NuaSpa.Application.Interfaces; // Ovo omogućava da vidi IReportingService
using NuaSpa.Application.DTOs;
using NuaSpa.Domain;
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

    public async Task<byte[]> GenerateTopUslugeReport()
    {
        // 1. Dohvati podatke (Top 5 po broju rezervacija)
        var data = await _context.Usluge
            .Select(u => new TopUslugaDTO
            {
                Naziv = u.Naziv,
                BrojRezervacija = _context.Rezervacije.Count(r => r.UslugaId == u.Id),
                UkupnaZarada = _context.Rezervacije.Count(r => r.UslugaId == u.Id) * u.Cijena
            })
            .OrderByDescending(x => x.BrojRezervacija)
            .Take(5)
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

        var ukupnoRezervacija = await _context.Rezervacije.CountAsync();
        var rezervacijeDanas = await _context.Rezervacije
            .Where(r => r.DatumRezervacije >= day && r.DatumRezervacije < next)
            .CountAsync();

        // Prihod računamo na osnovu plaćenih rezervacija (IsPlacena).
        var prihodDanas = await _context.Rezervacije
            .Where(r => r.IsPlacena && r.DatumRezervacije >= day && r.DatumRezervacije < next)
            .Join(
                _context.Usluge,
                r => r.UslugaId,
                u => u.Id,
                (r, u) => u.Cijena
            )
            .SumAsync(x => (decimal?)x) ?? 0m;

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

        var baseQuery = _context.Rezervacije
            .AsNoTracking()
            .Where(r => !r.IsOtkazana && r.DatumRezervacije >= dayStart && r.DatumRezervacije < next);

        int? noviKlijenti = null;
        decimal prihod = 0m;

        if (isAdmin)
        {
            noviKlijenti = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.DatumRegistracije >= registracijeOd);

            prihod = await baseQuery.Select(r => r.Usluga.Cijena).SumAsync();
        }
        else if (isZaposlenik)
        {
            prihod = await baseQuery
                .Where(r => r.ZaposlenikId == zaposlenikIdIfTherapist)
                .Select(r => r.Usluga.Cijena)
                .SumAsync();
        }
        else if (isKlijent)
        {
            prihod = await baseQuery
                .Where(r => r.KorisnikId == currentUserId)
                .Select(r => r.Usluga.Cijena)
                .SumAsync();
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
        var end = to.Date.AddDays(1);

        // Group by date, sum paid reservations.
        var points = await _context.Rezervacije
            .Where(r => r.IsPlacena && r.DatumRezervacije >= start && r.DatumRezervacije < end)
            .Join(
                _context.Usluge,
                r => r.UslugaId,
                u => u.Id,
                (r, u) => new { r.DatumRezervacije, u.Cijena }
            )
            .GroupBy(x => x.DatumRezervacije.Date)
            .Select(g => new RevenuePointDTO
            {
                Datum = g.Key,
                BrojRezervacija = g.Count(),
                Prihod = g.Sum(x => x.Cijena)
            })
            .OrderBy(p => p.Datum)
            .ToListAsync();

        return points;
    }

    public async Task<List<ServicePopularityDTO>> GetServicePopularityAsync(DateTime from, DateTime to, int take)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var safeTake = take <= 0 ? 8 : Math.Min(take, 30);

        var items = await _context.Rezervacije
            .Where(r => r.IsPlacena && r.DatumRezervacije >= start && r.DatumRezervacije < end)
            .Join(
                _context.Usluge,
                r => r.UslugaId,
                u => u.Id,
                (r, u) => new { u.Id, u.Naziv, u.Cijena }
            )
            .GroupBy(x => new { x.Id, x.Naziv })
            .Select(g => new ServicePopularityDTO
            {
                UslugaId = g.Key.Id,
                Naziv = g.Key.Naziv,
                BrojRezervacija = g.Count(),
                Prihod = g.Sum(x => x.Cijena),
            })
            .OrderByDescending(x => x.BrojRezervacija)
            .ThenByDescending(x => x.Prihod)
            .Take(safeTake)
            .ToListAsync();

        return items;
    }

    public async Task<List<TopSpenderDTO>> GetTopSpendersAsync(DateTime from, DateTime to, int take)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var safeTake = take <= 0 ? 10 : Math.Min(take, 50);

        var query = _context.Rezervacije
            .Where(r => r.IsPlacena && r.DatumRezervacije >= start && r.DatumRezervacije < end)
            .Join(
                _context.Usluge,
                r => r.UslugaId,
                u => u.Id,
                (r, u) => new { r.KorisnikId, r.DatumRezervacije, u.Cijena }
            )
            .GroupBy(x => x.KorisnikId)
            .Select(g => new
            {
                KorisnikId = g.Key,
                BrojPosjeta = g.Count(),
                Ukupno = g.Sum(x => x.Cijena),
                Zadnja = g.Max(x => x.DatumRezervacije)
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
}