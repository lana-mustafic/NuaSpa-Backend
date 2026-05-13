namespace NuaSpa.Application.DTOs;

public class RezervacijaCalendarItemDTO
{
    public int Id { get; set; }
    public DateTime DatumRezervacije { get; set; }
    public bool IsPotvrdjena { get; set; }
    public bool IsPlacena { get; set; }
    public bool IsOtkazana { get; set; }

    public int ZaposlenikId { get; set; }
    public string? ZaposlenikIme { get; set; }

    public int? ProstorijaId { get; set; }
    public string? ProstorijaNaziv { get; set; }

    public int KorisnikId { get; set; }
    public string? KorisnikIme { get; set; }
    public string? KorisnikTelefon { get; set; }
    public string? KorisnikEmail { get; set; }

    public int UslugaId { get; set; }
    public string? UslugaNaziv { get; set; }
    /// <summary>Trajanje tretmana u minutama (iz usluge).</summary>
    public int UslugaTrajanjeMinuta { get; set; }
    /// <summary>Redovna cijena usluge (ne mora odgovarati stvarnoj naplati).</summary>
    public decimal UslugaCijena { get; set; }

    /// <summary>
    /// VIP tretman (admin) — spremljeno na rezervaciji; kalendar prikazuje zlatni naglasak.
    /// </summary>
    public bool IsVip { get; set; }

    public string? RazlogOtkaza { get; set; }
}

