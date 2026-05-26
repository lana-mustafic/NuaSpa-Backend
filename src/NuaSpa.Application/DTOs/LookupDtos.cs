namespace NuaSpa.Application.DTOs;

public class DrzavaLookupDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
}

public class GradLookupDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
    public string PostanskiBroj { get; set; } = null!;
    public int DrzavaId { get; set; }
    public string? DrzavaNaziv { get; set; }
}
