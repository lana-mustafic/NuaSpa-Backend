using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class AdminReviewRowDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Ocjena { get; set; }
    public string Komentar { get; set; } = "";
    public string KorisnikPunoIme { get; set; } = "";
    public int BrojPosjeta { get; set; }
    public string UslugaNaziv { get; set; } = "";
    public string? TerapeutIme { get; set; }
    /// <summary>Recenzije iz aplikacije (nema zasebnog polja u bazi).</summary>
    public string Izvor { get; set; } = "NuaSpa";
}

public class AdminTopUslugaOcjenaDto
{
    public string Naziv { get; set; } = "";
    public double Prosjek { get; set; }
}

public class AdminReviewQuoteDto
{
    public string Tekst { get; set; } = "";
    public string Autor { get; set; } = "";
    public int Ocjena { get; set; }
}

/// <summary>Admin pregled recenzija: KPI, distribucija, tablica (paginacija).</summary>
public class AdminReviewsDashboardDto
{
    public int Ukupno { get; set; }
    public int Stranica { get; set; }
    public int VelicinaStranice { get; set; }

    public List<AdminReviewRowDto> Redovi { get; set; } = new();

    public double ProsjecnaOcjena { get; set; }
    public double? ProsjecnaOcjenaPrethodno { get; set; }

    public int UkupnoPrethodno { get; set; }

    /// <summary>Postotak recenzija s ocjenom 4 ili 5.</summary>
    public double PostotakPozitivnih { get; set; }

    public double? PostotakPozitivnihPrethodno { get; set; }

    /// <summary>Null dok ne postoji model odgovora u bazi.</summary>
    public double? PostotakOdgovora { get; set; }

    /// <summary>Ključ: broj zvjezdica (1–5), vrijednost: broj recenzija.</summary>
    public Dictionary<int, int> DistribucijaOcjena { get; set; } = new();

    public List<AdminTopUslugaOcjenaDto> TopUsluge { get; set; } = new();

    public AdminReviewQuoteDto? IstaknutaRecenzija { get; set; }
}
