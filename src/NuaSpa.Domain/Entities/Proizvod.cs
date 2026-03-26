using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Proizvod : BaseEntity
    {
        public string Naziv { get; set; } = null!;
        public string Sifra { get; set; } = null!; // npr. SKU-123
        public string Opis { get; set; } = null!;
        public decimal Cijena { get; set; }
        public byte[]? Slika { get; set; }
    }
}
