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

    public string? KorisnikIme { get; set; }
    public string? UslugaNaziv { get; set; }
}

