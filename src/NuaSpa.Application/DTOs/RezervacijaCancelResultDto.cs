namespace NuaSpa.Application.DTOs;

public class RezervacijaCancelResultDto
{
    public bool Otkazana { get; set; }
    public bool RefundIzvrsen { get; set; }
    public decimal? RefundiraniIznos { get; set; }
    public string? RefundId { get; set; }
}
