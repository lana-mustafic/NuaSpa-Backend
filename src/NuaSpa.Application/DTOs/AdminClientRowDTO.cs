namespace NuaSpa.Application.DTOs;

public class AdminClientRowDTO
{
    public int Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime DatumRegistracije { get; set; }

    public DateTime? ZadnjaPosjeta { get; set; }
    public int UkupnoPosjeta { get; set; }
    public decimal UkupnoPotroseno { get; set; }
    public bool IsVip { get; set; }
}

