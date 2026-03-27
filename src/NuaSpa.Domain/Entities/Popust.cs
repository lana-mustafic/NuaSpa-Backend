using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities;

public class Popust : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Naziv { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Procenat { get; set; }

    [Required]
    public DateTime VrijediOd { get; set; }

    [Required]
    public DateTime VrijediDo { get; set; }

    [Required]
    public bool IsAktivan { get; set; } = true;
}