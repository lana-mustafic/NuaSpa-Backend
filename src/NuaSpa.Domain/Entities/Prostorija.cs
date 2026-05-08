using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Prostorija : BaseEntity
{
    [Required]
    [ForeignKey("SpaCentar")]
    public int SpaCentarId { get; set; }
    public SpaCentar SpaCentar { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string Naziv { get; set; } = null!;

    [MaxLength(400)]
    public string? Opis { get; set; }

    public int Kapacitet { get; set; } = 1;

    public bool IsAktivna { get; set; } = true;
}

