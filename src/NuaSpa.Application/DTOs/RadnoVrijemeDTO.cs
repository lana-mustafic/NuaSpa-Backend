namespace NuaSpa.Application.DTOs;

public class RadnoVrijemeDTO
{
    public int Id { get; set; }
    public int SpaCentarId { get; set; }
    public int DanUSedmici { get; set; }
    public bool IsClosed { get; set; }
    public int? OtvaraMin { get; set; }
    public int? ZatvaraMin { get; set; }
}

