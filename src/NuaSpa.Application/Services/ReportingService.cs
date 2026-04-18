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
}