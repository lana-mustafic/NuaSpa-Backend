using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Entities
{
    public class Rezervacija : BaseEntity
    {
        [Required]
        public DateTime DatumRezervacije { get; set; }

        [Required]
        public RezervacijaStatus Status { get; set; } = RezervacijaStatus.Pending;

        [Required]
        public bool IsPotvrdjena { get; set; }

        [Required]
        public bool IsPlacena { get; set; }

        public bool IsOtkazana { get; set; }

        /// <summary>Snimak cijene usluge u trenutku kreiranja rezervacije.</summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SnimakCijena { get; set; }

        /// <summary>Snimak trajanja usluge (minute) u trenutku kreiranja.</summary>
        [Required]
        public int SnimakTrajanjeMinuta { get; set; }

        /// <summary>
        /// VIP tretman (admin) — poseban prikaz u kalendaru i sl.
        /// </summary>
        public bool IsVip { get; set; }

        [MaxLength(400)]
        public string? RazlogOtkaza { get; set; }

        public DateTime? OtkazanaAt { get; set; }

        public int? PotvrdioUserId { get; set; }

        public DateTime? PotvrdjenaAt { get; set; }

        public int? OtkazaoUserId { get; set; }

        public int? ZavrsioUserId { get; set; }

        public DateTime? ZavrsenaAt { get; set; }

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

        public ICollection<RezervacijaStatusPromjena> StatusPromjene { get; set; } =
            new List<RezervacijaStatusPromjena>();
    }
}