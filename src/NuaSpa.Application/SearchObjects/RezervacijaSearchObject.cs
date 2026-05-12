using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.SearchObjects
{
    public class RezervacijaSearchObject
    {
        public int? KorisnikId { get; set; }

        /// <summary>Samo Admin: filtriraj rezervacije po terapeutu.</summary>
        public int? ZaposlenikId { get; set; }

        public DateTime? Datum { get; set; }
        public bool? IsPotvrdjena { get; set; }
        public bool? IncludeOtkazane { get; set; }
    }
}
