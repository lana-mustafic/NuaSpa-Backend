using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Application.DTOs
{
    public class KorisnikDTO
    {
        public int Id { get; set; }
        public string Ime { get; set; } = null!;
        public string Prezime { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telefon { get; set; } = null!;
        public string KorisnickoIme { get; set; } = null!;

        // Često dodajemo i naziv uloge direktno ovdje
        public string? UlogaNaziv { get; set; }
    }
}
