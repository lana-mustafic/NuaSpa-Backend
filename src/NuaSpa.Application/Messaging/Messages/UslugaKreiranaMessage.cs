namespace NuaSpa.Application.Messaging.Messages;

public sealed class UslugaKreiranaMessage
{
    public int UslugaId { get; set; }
    public string Naziv { get; set; } = null!;
    public string KategorijaNaziv { get; set; } = null!;
    public decimal Cijena { get; set; }
    public string? AdminNotifyEmail { get; set; }
}
