using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Entities;

public class Placanje : BaseEntity
{
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    /// <summary>Stvarno naplaćeni iznos (iz Stripe PaymentIntent), ako se razlikuje od kataloga.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? NaplaceniIznos { get; set; }

    [Required]
    public DateTime DatumPlacanja { get; set; }

    public DateTime? DatumZavrsetka { get; set; }

    [Required]
    [MaxLength(50)]
    public string MetodaPlacanja { get; set; } = "Gotovina";

    [Required]
    [MaxLength(100)]
    public string TransakcijskiBroj { get; set; } = null!;

    [MaxLength(100)]
    public string? StripeRefundId { get; set; }

    public int? RefundedByUserId { get; set; }

    public DateTime? RefundedAtUtc { get; set; }

    public PlacanjeStatus Status { get; set; } = PlacanjeStatus.Pending;

    [ForeignKey("Rezervacija")]
    public int? RezervacijaId { get; set; }
    public Rezervacija? Rezervacija { get; set; }
}