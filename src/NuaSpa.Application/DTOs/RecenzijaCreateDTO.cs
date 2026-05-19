namespace NuaSpa.Application.DTOs
{
    public class RecenzijaCreateDTO
    {
        public int UslugaId { get; set; }
        public int ZaposlenikId { get; set; }
        public int Ocjena { get; set; }
        public string Komentar { get; set; } = null!;
    }
}

