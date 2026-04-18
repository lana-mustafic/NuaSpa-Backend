namespace NuaSpa.Application.DTOs
{
    // Mora biti PUBLIC i mora imati polja
    public class TopUslugaDTO
    {
        public string Naziv { get; set; } = null!;
        public int BrojRezervacija { get; set; }
        public decimal UkupnaZarada { get; set; }
    }
}