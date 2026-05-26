namespace NuaSpa.Application.Messaging.Messages;

public sealed class RezervacijaPotvrdaMessage
{
    public int RezervacijaId { get; set; }
    public string ToEmail { get; set; } = null!;
    public string KorisnikIme { get; set; } = null!;
    public string UslugaNaziv { get; set; } = null!;
    public string TerapeutIme { get; set; } = null!;
    public DateTime DatumRezervacije { get; set; }
    public decimal Cijena { get; set; }
    public bool IsPotvrdjena { get; set; }
}
