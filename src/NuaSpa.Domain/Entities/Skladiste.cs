using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Skladiste : BaseEntity
    {
        [Required]
        public int KolicinaNaStanju { get; set; }

        [Required]
        [MaxLength(200)]
        public string Lokacija { get; set; } = "Glavno Skladište";

        [Required]
        [ForeignKey("Proizvod")]
        public int ProizvodId { get; set; }
        public Proizvod Proizvod { get; set; } = null!;
    }
}