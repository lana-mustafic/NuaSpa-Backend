namespace NuaSpa.Application.DTOs;

public class DrzavaLookupDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
    public string PozivniBroj { get; set; } = string.Empty;
}

public class GradLookupDto
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
    public string PostanskiBroj { get; set; } = null!;
    public int DrzavaId { get; set; }
    public string? DrzavaNaziv { get; set; }
}

public class DrzavaWriteDto
{
    public string Naziv { get; set; } = string.Empty;
    public string PozivniBroj { get; set; } = string.Empty;
}

public class GradWriteDto
{
    public string Naziv { get; set; } = string.Empty;
    public string PostanskiBroj { get; set; } = string.Empty;
    public int DrzavaId { get; set; }
}
