using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Usluga : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Naziv { get; set; } = null!;

        [MaxLength(1000)]
        public string Opis { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cijena { get; set; }

        [Required]
        public int TrajanjeMinuta { get; set; }

        [Required]
        [ForeignKey("KategorijaUsluga")]
        public int KategorijaUslugaId { get; set; }
        public KategorijaUsluga KategorijaUsluga { get; set; } = null!;
    }
}