using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.SearchObjects
{
    public class RezervacijaSearchObject
    {
        public int? KorisnikId { get; set; }
        public DateTime? Datum { get; set; }
        public bool? IsPotvrdjena { get; set; }
    }
}
