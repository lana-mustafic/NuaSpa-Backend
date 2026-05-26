namespace NuaSpa.Application.Messaging.Messages;

public sealed class RezervacijaOtkazanaMessage
{
    public int RezervacijaId { get; set; }
    public string ToEmail { get; set; } = null!;
    public string KorisnikIme { get; set; } = null!;
    public string UslugaNaziv { get; set; } = null!;
    public DateTime DatumRezervacije { get; set; }
    public string RazlogOtkaza { get; set; } = null!;
    public string OtkazaoUloga { get; set; } = null!;
}
