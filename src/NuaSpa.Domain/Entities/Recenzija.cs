using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Recenzija : BaseEntity
    {
        [Required]
        [Range(1, 5)]
        public int Ocjena { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Komentar { get; set; } = null!;

        [Required]
        [ForeignKey("Usluga")]
        public int UslugaId { get; set; }
        public Usluga Usluga { get; set; } = null!;

        [Required]
        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = null!;
    }
}