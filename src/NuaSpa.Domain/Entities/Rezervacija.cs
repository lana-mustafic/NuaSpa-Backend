using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Rezervacija : BaseEntity
    {
        public DateTime DatumRezervacije { get; set; }
        public bool IsPotvrdjena { get; set; }
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = null!;
        public int UslugaId { get; set; }
        public Usluga Usluga { get; set; } = null!;

        public int ZaposlenikId { get; set; }
        public Zaposlenik Zaposlenik { get; set; } = null!;
    }
}
