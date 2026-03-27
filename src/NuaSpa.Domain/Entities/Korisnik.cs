using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using NuaSpa.Domain.Common;

namespace NuaSpa.Domain.Entities
{
    public class Korisnik : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ime { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Prezime { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        public string Telefon { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string KorisnickoIme { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string PasswordSalt { get; set; } = null!;

        [Required]
        public bool Status { get; set; } = true;

        // --- Relacije ---

        [Required]
        public int GradId { get; set; }

        [ForeignKey("GradId")]
        public virtual Grad Grad { get; set; } = null!;

        [Required]
        public int UlogaId { get; set; }

        [ForeignKey("UlogaId")]
        public virtual Uloga Uloga { get; set; } = null!;
    }
}