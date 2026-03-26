using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class NarudzbaProizvoda : BaseEntity
{
    public int Kolicina { get; set; }
    public decimal UkupnaCijena { get; set; }

    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;

    public int ProizvodId { get; set; }
    public Proizvod Proizvod { get; set; } = null!;
}