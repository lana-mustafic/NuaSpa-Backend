using System;

namespace NuaSpa.Application.DTOs;

/// <summary>Recenzija vezana uz termine gdje je terapeut radio (isti klijent + usluga).</summary>
public class TherapistReviewRowDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string KorisnikIme { get; set; } = null!;
    public int Ocjena { get; set; }
    public string Komentar { get; set; } = null!;
    public string UslugaNaziv { get; set; } = null!;
    public string? AdminOdgovor { get; set; }
}
