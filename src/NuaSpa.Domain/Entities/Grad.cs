using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NuaSpa.Domain.Common;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Grad : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Naziv { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string PostanskiBroj { get; set; } = null!;

        [Required]
        [ForeignKey("Drzava")]
        public int DrzavaId { get; set; }
        public Drzava Drzava { get; set; } = null!;
    }
}