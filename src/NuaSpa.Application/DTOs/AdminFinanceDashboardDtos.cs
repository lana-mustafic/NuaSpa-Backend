using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class AdminFinanceDashboardDto
{
    public AdminFinanceKpiDto Kpi { get; set; } = null!;
    public IList<AdminFinanceTransactionRowDto> Redovi { get; set; } = Array.Empty<AdminFinanceTransactionRowDto>();
    public int Ukupno { get; set; }
    public int Stranica { get; set; }
    public int VelicinaStranice { get; set; }
    public IList<AdminFinanceMethodShareDto> MetodePostotak { get; set; } = Array.Empty<AdminFinanceMethodShareDto>();
    public IList<AdminFinanceTrendPointDto> PrihodDnevno { get; set; } = Array.Empty<AdminFinanceTrendPointDto>();
    public IList<AdminFinanceActivityDto> NedavnaAktivnost { get; set; } = Array.Empty<AdminFinanceActivityDto>();
}

public class AdminFinanceKpiDto
{
    public decimal UkupniPrihod { get; set; }
    public double? PostotakPromjeneUkupniPrihod { get; set; }

    public int PlaceneRezervacije { get; set; }
    public double? PostotakPromjenePlaceneRezervacije { get; set; }

    public decimal ProsjecnaVrijednost { get; set; }
    public double? PostotakPromjeneProsjecnaVrijednost { get; set; }

    public int NeplaceneRezervacije { get; set; }
    public double? PostotakPromjeneNeplaceneRezervacije { get; set; }

    public decimal IznosRefundacija { get; set; }
    public double? PostotakPromjeneRefundacija { get; set; }
}

public class AdminFinanceTransactionRowDto
{
    public int PlacanjeId { get; set; }
    public int? RezervacijaId { get; set; }
    public string TransakcijskiId { get; set; } = null!;
    public string? StripePaymentIntentId { get; set; }
    public string KlijentPunoIme { get; set; } = null!;
    public string UslugaTekst { get; set; } = null!;
    public DateTime DatumVrijeme { get; set; }
    public DateTime? DatumZavrsetka { get; set; }
    public decimal Iznos { get; set; }
    public decimal? NaplaceniIznos { get; set; }
    public string MetodaLabel { get; set; } = null!;
    public string? StripeRefundId { get; set; }
    /// <summary>paid | unpaid | failed | refunded</summary>
    public string Status { get; set; } = null!;
}

public class AdminFinanceCsvResultDto
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public bool Truncated { get; set; }
    public int ExportedRows { get; set; }
    public int TotalMatchingRows { get; set; }
}

public class AdminFinanceMethodShareDto
{
    public string Kljuc { get; set; } = null!;
    public string Label { get; set; } = null!;
    public double Postotak { get; set; }
}

public class AdminFinanceTrendPointDto
{
    public DateTime Datum { get; set; }
    public decimal Iznos { get; set; }
}

public class AdminFinanceActivityDto
{
    public string Tip { get; set; } = null!;
    public string Opis { get; set; } = null!;
    public string Klijent { get; set; } = null!;
    public decimal Iznos { get; set; }
    public DateTime DatumVrijeme { get; set; }
}
