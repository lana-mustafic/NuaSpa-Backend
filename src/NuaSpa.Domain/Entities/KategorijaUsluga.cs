using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class KategorijaUsluga : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Naziv { get; set; } = null!;

        [MaxLength(500)]
        public string? Opis { get; set; }
    }
}