using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Rezervacija : BaseEntity
    {
        [Required]
        public DateTime DatumRezervacije { get; set; }

        [Required]
        public bool IsPotvrdjena { get; set; }

        [Required]
        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = null!;

        [Required]
        [ForeignKey("Usluga")]
        public int UslugaId { get; set; }
        public Usluga Usluga { get; set; } = null!;

        [Required]
        [ForeignKey("Zaposlenik")]
        public int ZaposlenikId { get; set; }
        public Zaposlenik Zaposlenik { get; set; } = null!;
    }
}