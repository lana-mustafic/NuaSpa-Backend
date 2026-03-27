using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Zaposlenik : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ime { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Prezime { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specijalizacija { get; set; } = null!;

        [Required]
        public DateTime DatumZaposlenja { get; set; }
    }
}