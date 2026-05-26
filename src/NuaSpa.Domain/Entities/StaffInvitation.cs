using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NuaSpa.Domain.Entities;

/// <summary>
/// One-time therapist portal activation invite (admin-provisioned login).
/// </summary>
public class StaffInvitation
{
    public int Id { get; set; }

    [Required]
    public int ZaposlenikId { get; set; }

    [ForeignKey(nameof(ZaposlenikId))]
    public virtual Zaposlenik Zaposlenik { get; set; } = null!;

    [Required]
    public int KorisnikId { get; set; }

    [ForeignKey(nameof(KorisnikId))]
    public virtual Korisnik Korisnik { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    /// <summary>SHA-256 hex of the raw invite token (never store raw token).</summary>
    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public int? CreatedByKorisnikId { get; set; }

    [ForeignKey(nameof(CreatedByKorisnikId))]
    public virtual Korisnik? CreatedByKorisnik { get; set; }
}
