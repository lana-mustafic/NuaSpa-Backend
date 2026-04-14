using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NuaSpa.Domain.Entities
{
    // Nasljeđujemo IdentityRole<int> koji već sadrži:
    // Id i Name (što mijenja tvoj Naziv)
    public class Uloga : IdentityRole<int>
    {
        // Konstruktor je potreban da bi lakše kreirali uloge
        public Uloga() : base() { }

        public Uloga(string naziv) : base(naziv)
        {
            // IdentityRole koristi polje "Name" za naziv uloge
        }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Ako imaš neki specifičan opis uloge, možeš ga dodati ovdje
        [MaxLength(200)]
        public string? Opis { get; set; }
    }
}