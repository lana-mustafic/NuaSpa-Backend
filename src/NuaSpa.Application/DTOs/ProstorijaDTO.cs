namespace NuaSpa.Application.DTOs;

public class ProstorijaDTO
{
    public int Id { get; set; }
    public int SpaCentarId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public int Kapacitet { get; set; }
    public bool IsAktivna { get; set; }
}

