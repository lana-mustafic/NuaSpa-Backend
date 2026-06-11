using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NuaSpa.Domain.Common;
using NuaSpa.Domain.Enums;

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
        [MaxLength(500)]
        public string Specijalizacija { get; set; } = null!;

        [MaxLength(30)]
        public string? Telefon { get; set; }

        [MaxLength(120)]
        public string? Email { get; set; }

        /// <summary>Primarna kategorija usluga za koju je terapeut predviden.</summary>
        public int? KategorijaUslugaId { get; set; }

        public virtual KategorijaUsluga? KategorijaUsluga { get; set; }

        [MaxLength(200)]
        public string? Jezici { get; set; }

        [MaxLength(1000)]
        public string? Obrazovanje { get; set; }

        [MaxLength(120)]
        public string? Lokacija { get; set; }

        /// <summary>Short therapist bio shown on profile (self-service editable).</summary>
        [MaxLength(2000)]
        public string? Bio { get; set; }

        [Required]
        public DateTime DatumZaposlenja { get; set; }

        /// <summary>Active therapists can be booked; Inactive/OnLeave are hidden from booking.</summary>
        public ZaposlenikStatus Status { get; set; } = ZaposlenikStatus.Active;
    }
}
