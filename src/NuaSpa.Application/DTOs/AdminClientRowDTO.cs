namespace NuaSpa.Application.DTOs;

public class AdminClientRowDTO
{
    public int Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Login username (read-only in admin UI).</summary>
    public string UserName { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;
    public DateTime DatumRegistracije { get; set; }

    public DateTime? ZadnjaPosjeta { get; set; }
    public int UkupnoPosjeta { get; set; }
    public decimal UkupnoPotroseno { get; set; }
    public bool IsVip { get; set; }

    public bool IsVipKlijent { get; set; }

    /// <summary>Account active (Korisnik.Status).</summary>
    public bool Status { get; set; } = true;

    public int GradId { get; set; }

    public string? GradNaziv { get; set; }

    /// <summary>Client notes for therapists (not staff internal notes).</summary>
    public string? NapomenaZaTerapeuta { get; set; }
}
