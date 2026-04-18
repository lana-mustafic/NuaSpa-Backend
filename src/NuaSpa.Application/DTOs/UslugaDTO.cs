namespace NuaSpa.Application.DTOs
{
    public class UslugaDTO
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = null!;
        public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
        public string Opis { get; set; } = null!;

        // DODAJ OVO LINIJU:
        public int KategorijaUslugaId { get; set; }
    }
}