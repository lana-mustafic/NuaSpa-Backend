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
        public bool IsPlacena { get; set; }

        public bool IsOtkazana { get; set; }

        [MaxLength(400)]
        public string? RazlogOtkaza { get; set; }

        public DateTime? OtkazanaAt { get; set; }

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

        [ForeignKey("Prostorija")]
        public int? ProstorijaId { get; set; }
        public Prostorija? Prostorija { get; set; }

        public ICollection<RezervacijaOprema> RezervacijaOprema { get; set; } =
            new List<RezervacijaOprema>();
    }
}