using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class RezervacijaOprema : BaseEntity
{
    [Required]
    [ForeignKey("Rezervacija")]
    public int RezervacijaId { get; set; }
    public Rezervacija Rezervacija { get; set; } = null!;

    [Required]
    [ForeignKey("Oprema")]
    public int OpremaId { get; set; }
    public Oprema Oprema { get; set; } = null!;

    public int Kolicina { get; set; } = 1;
}

