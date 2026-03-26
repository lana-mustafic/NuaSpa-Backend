using NuaSpa.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuaSpa.Domain.Entities
{
    public class Korisnik : BaseEntity
    {
        public string Ime { get; set; } = null!;
        public string Prezime { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telefon { get; set; } = null!;
        public string KorisnickoIme { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public bool Status { get; set; } = true;

        // Veza sa Ulogom 
        public int UlogaId { get; set; }
        public Uloga Uloga { get; set; } = null!;
    }
}
