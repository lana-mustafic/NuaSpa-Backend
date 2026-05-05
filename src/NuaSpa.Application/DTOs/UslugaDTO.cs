namespace NuaSpa.Application.DTOs
{
    public class UslugaDTO
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = null!;
        public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
        public string Opis { get; set; } = null!;
        public int KategorijaUslugaId { get; set; }

        /// <summary>Naziv kategorije (read-only u JSON odgovoru).</summary>
        public string? KategorijaNaziv { get; set; }

        /// <summary>Čitljivo trajanje za klijente (npr. "60 min").</summary>
        public string? TrajanjeTekst { get; set; }

        public string? SlikaUrl { get; set; }
    }
}