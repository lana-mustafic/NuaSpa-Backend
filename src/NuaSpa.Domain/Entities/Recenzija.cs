using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Recenzija : BaseEntity
    {
        public int Ocjena { get; set; } // 1 do 5
        public string Komentar { get; set; } = null!;

        // Na koju se uslugu odnosi?
        public int UslugaId { get; set; }
        public Usluga Usluga { get; set; } = null!;

        // Ko je ostavio recenziju?
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = null!;
    }
}
