using System.ComponentModel.DataAnnotations;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class SpaCentar : BaseEntity
{
    [Required]
    [MaxLength(120)]
    public string Naziv { get; set; } = "NuaSpa";

    [MaxLength(200)]
    public string? Adresa { get; set; }

    [MaxLength(120)]
    public string? Email { get; set; }

    [MaxLength(60)]
    public string? Telefon { get; set; }

    [MaxLength(1200)]
    public string? Opis { get; set; }
}

