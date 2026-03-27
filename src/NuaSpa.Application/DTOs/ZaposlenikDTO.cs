namespace NuaSpa.Application.DTOs;

public class ZaposlenikDTO
{
    public int Id { get; set; }
    public string Ime { get; set; } = null!;
    public string Prezime { get; set; } = null!;
    public string Specijalizacija { get; set; } = null!;
    public string? Telefon { get; set; }
}