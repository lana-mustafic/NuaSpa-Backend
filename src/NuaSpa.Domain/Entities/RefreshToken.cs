namespace NuaSpa.Domain.Entities;

/// <summary>
/// Rotating opaque refresh token (stored hashed). Used to obtain new access JWTs without re-login.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public Korisnik User { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public Guid FamilyId { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public int? ReplacedById { get; set; }

    public RefreshToken? ReplacedBy { get; set; }
}
