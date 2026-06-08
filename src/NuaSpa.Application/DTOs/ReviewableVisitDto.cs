using System;

namespace NuaSpa.Application.DTOs;

public class ReviewableVisitDto
{
    public int RezervacijaId { get; set; }
    public int ZaposlenikId { get; set; }
    public string ZaposlenikIme { get; set; } = null!;
    public DateTime DatumRezervacije { get; set; }
}
