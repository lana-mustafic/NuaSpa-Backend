using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class RadnoVrijeme : BaseEntity
{
    [Required]
    [ForeignKey("SpaCentar")]
    public int SpaCentarId { get; set; }
    public SpaCentar SpaCentar { get; set; } = null!;

    // 1 = Monday ... 7 = Sunday
    [Range(1, 7)]
    public int DanUSedmici { get; set; }

    public bool IsClosed { get; set; }

    // minutes since midnight; null when closed
    public int? OtvaraMin { get; set; }
    public int? ZatvaraMin { get; set; }
}

