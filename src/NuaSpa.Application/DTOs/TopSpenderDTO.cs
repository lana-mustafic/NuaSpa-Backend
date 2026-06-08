namespace NuaSpa.Application.DTOs;

public class TopSpenderDTO
{
    public int KorisnikId { get; set; }
    public string ImePrezime { get; set; } = string.Empty;
    public string? Email { get; set; }
    /// <summary>Broj uspješnih uplata u periodu (legacy naziv polja).</summary>
    public int BrojPosjeta { get; set; }

    public int BrojPlacanja { get; set; }
    public decimal UkupnoPotroseno { get; set; }
    public DateTime? ZadnjaPosjeta { get; set; }
}

