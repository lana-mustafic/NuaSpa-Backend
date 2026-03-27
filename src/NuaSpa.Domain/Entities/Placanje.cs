using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Placanje : BaseEntity
{
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    [Required]
    public DateTime DatumPlacanja { get; set; }

    [Required]
    [MaxLength(50)]
    public string MetodaPlacanja { get; set; } = "Gotovina";

    [Required]
    [MaxLength(100)]
    public string TransakcijskiBroj { get; set; } = null!;

    [ForeignKey("Rezervacija")]
    public int? RezervacijaId { get; set; }
    public Rezervacija? Rezervacija { get; set; }
}