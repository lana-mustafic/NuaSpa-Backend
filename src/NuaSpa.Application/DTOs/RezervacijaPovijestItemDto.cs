namespace NuaSpa.Application.DTOs;

public class RezervacijaPovijestItemDto
{
    public int Id { get; set; }
    public DateTime DatumRezervacije { get; set; }
    public string? UslugaNaziv { get; set; }
    public bool IsPotvrdjena { get; set; }
    public bool IsPlacena { get; set; }
    public bool IsOtkazana { get; set; }

    /// <summary>Pending, Confirmed, Cancelled, Completed.</summary>
    public string Status { get; set; } = "Pending";
}
