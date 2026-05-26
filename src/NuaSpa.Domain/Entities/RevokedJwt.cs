namespace NuaSpa.Domain.Entities;

/// <summary>Opozvani JWT (logout) — JTI se čuva do isteka tokena.</summary>
public class RevokedJwt
{
    public int Id { get; set; }
    public string Jti { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime RevokedAtUtc { get; set; }
}
