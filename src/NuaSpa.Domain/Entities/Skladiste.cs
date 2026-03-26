using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Skladiste : BaseEntity
    {
        public int KolicinaNaStanju { get; set; }
        public string Lokacija { get; set; } = "Glavno Skladište";

        // Veza sa Proizvodom
        public int ProizvodId { get; set; }
        public Proizvod Proizvod { get; set; } = null!;
    }
}
