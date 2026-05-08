namespace NuaSpa.Application.DTOs;

public class SpaCentarDTO
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Adresa { get; set; }
    public string? Email { get; set; }
    public string? Telefon { get; set; }
    public string? Opis { get; set; }
}

