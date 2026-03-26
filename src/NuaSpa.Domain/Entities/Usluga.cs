using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Usluga : BaseEntity
    {
        public string Naziv { get; set; } = null!;
        public string Opis { get; set; } = null!;
            public decimal Cijena { get; set; }
        public int TrajanjeMinuta { get; set; }
            public int KategorijaUslugaId { get; set; }
        public KategorijaUsluga KategorijaUsluga { get; set; } = null!;
    }
}
