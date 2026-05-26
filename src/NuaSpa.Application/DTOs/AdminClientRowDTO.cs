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

    /// <summary>Preferirani terapeut (Korisnik.ZaposlenikId).</summary>
    public int? PreferiraniZaposlenikId { get; set; }

    /// <summary>Terapeut prikazan u tablici: preferirani ako postoji, inače zadnji s neotkazane posjete.</summary>
    public int? TerapeutZaposlenikId { get; set; }

    public string? TerapeutIme { get; set; }
    public string? TerapeutPrezime { get; set; }

    public bool IsVipKlijent { get; set; }

    /// <summary>Account active (Korisnik.Status).</summary>
    public bool Status { get; set; } = true;

    public int GradId { get; set; }

    public string? GradNaziv { get; set; }
}

