using System;
using System.ComponentModel.DataAnnotations;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

/// <summary>Objavljena novost/obavijest (news feed).</summary>
public class Obavijest : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Naslov { get; set; } = null!;

    [Required]
    [MaxLength(8000)]
    public string Tekst { get; set; } = null!;

    [MaxLength(500)]
    public string? SlikaUrl { get; set; }

    public DateTime DatumObjave { get; set; }

    public bool Aktivna { get; set; } = true;
}
