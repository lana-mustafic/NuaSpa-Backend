using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Entities;

/// <summary>Audit trag promjene statusa rezervacije.</summary>
public class RezervacijaStatusPromjena : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Rezervacija))]
    public int RezervacijaId { get; set; }

    public Rezervacija Rezervacija { get; set; } = null!;

    public RezervacijaStatus FromStatus { get; set; }

    public RezervacijaStatus ToStatus { get; set; }

    [Required]
    public int ActorUserId { get; set; }

    [MaxLength(400)]
    public string? Opis { get; set; }
}
