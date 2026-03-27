namespace NuaSpa.Application.DTOs;

public class ProizvodDTO
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
    public string Sifra { get; set; } = null!;
    public decimal Cijena { get; set; }
    public byte[]? Slika { get; set; }
}