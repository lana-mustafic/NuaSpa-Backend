using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NuaSpa.Domain.Entities
{
    // Nasljeđujemo IdentityUser<int> koji već sadrži:
    // Id, UserName, Email, PhoneNumber, PasswordHash, SecurityStamp itd.
    public class Korisnik : IdentityUser<int>
    {
        [Required]
        [MaxLength(50)]
        public string Ime { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Prezime { get; set; } = null!;

        [Required]
        public bool Status { get; set; } = true;

        public DateTime DatumRegistracije { get; set; } = DateTime.Now;

        // --- Relacije ---

        [Required]
        public int GradId { get; set; }

        [ForeignKey("GradId")]
        public virtual Grad Grad { get; set; } = null!;

        public int? ZaposlenikId { get; set; }

        [ForeignKey("ZaposlenikId")]
        public virtual Zaposlenik? Zaposlenik { get; set; }

        // Napomena: UlogaId je uklonjen jer Identity koristi tabelu AspNetUserRoles 
        // za povezivanje korisnika i uloga.
    }
}