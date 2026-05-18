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
        [MaxLength(500)]
        public string Specijalizacija { get; set; } = null!;

        [MaxLength(30)]
        public string? Telefon { get; set; }

        /// <summary>Primarna kategorija usluga za koju je terapeut predviđen.</summary>
        public int? KategorijaUslugaId { get; set; }

        public virtual KategorijaUsluga? KategorijaUsluga { get; set; }

        [MaxLength(200)]
        public string? Jezici { get; set; }

        [MaxLength(1000)]
        public string? Obrazovanje { get; set; }

        [MaxLength(120)]
        public string? Lokacija { get; set; }

        [Required]
        public DateTime DatumZaposlenja { get; set; }
    }
}