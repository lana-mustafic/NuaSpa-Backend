using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Placanje : BaseEntity
{
    public decimal Iznos { get; set; }
    public DateTime DatumPlacanja { get; set; }
    public string MetodaPlacanja { get; set; } = "Gotovina"; // Gotovina, Kartica, Online
    public string TransakcijskiBroj { get; set; } = null!;

    // Veza sa rezervacijom (ako je plaćanje za termin)
    public int? RezervacijaId { get; set; }
    public Rezervacija? Rezervacija { get; set; }
}