namespace NuaSpa.Application.DTOs;

public class ServicePopularityDTO
{
    public int UslugaId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    /// <summary>Broj uspješnih uplata u periodu (legacy naziv polja).</summary>
    public int BrojRezervacija { get; set; }

    public int BrojPlacanja { get; set; }
    public decimal Prihod { get; set; }
}

