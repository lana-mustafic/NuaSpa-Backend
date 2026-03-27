namespace NuaSpa.Application.DTOs;

public class PopustDTO
{
    public int Id { get; set; }
    public string Naziv { get; set; } = null!;
    public decimal Procenat { get; set; }
    public DateTime DatumIsteka { get; set; }
    public bool IsAktivan { get; set; }
}