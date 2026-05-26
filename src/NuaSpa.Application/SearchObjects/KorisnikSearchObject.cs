using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.SearchObjects
{
    public class KorisnikSearchObject
    {
        /// <summary>Opća pretraga (ime, prezime, email, telefon, username).</summary>
        public string? Q { get; set; }

        public string? Ime { get; set; }
        public string? Prezime { get; set; }
    }
}
