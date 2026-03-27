using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class NarudzbaProizvoda : BaseEntity
{
    [Required]
    public int Kolicina { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UkupnaCijena { get; set; }

    [Required]
    [ForeignKey("Korisnik")]
    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;

    [Required]
    [ForeignKey("Proizvod")]
    public int ProizvodId { get; set; }
    public Proizvod Proizvod { get; set; } = null!;
}