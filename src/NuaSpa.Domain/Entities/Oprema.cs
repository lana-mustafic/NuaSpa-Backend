using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Oprema : BaseEntity
{
    [Required]
    [ForeignKey("SpaCentar")]
    public int SpaCentarId { get; set; }
    public SpaCentar SpaCentar { get; set; } = null!;

    [Required]
    [MaxLength(120)]
    public string Naziv { get; set; } = null!;

    [MaxLength(400)]
    public string? Napomena { get; set; }

    public int Kolicina { get; set; } = 1;

    public bool IsIspravna { get; set; } = true;
}

