namespace NuaSpa.Application.DTOs;

public class ServicePopularityDTO
{
    public int UslugaId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public int BrojRezervacija { get; set; }
    public decimal Prihod { get; set; }
}

