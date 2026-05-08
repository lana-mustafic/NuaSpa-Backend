namespace NuaSpa.Application.DTOs;

public class OpremaDTO
{
    public int Id { get; set; }
    public int SpaCentarId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Napomena { get; set; }
    public int Kolicina { get; set; }
    public bool IsIspravna { get; set; }
}

