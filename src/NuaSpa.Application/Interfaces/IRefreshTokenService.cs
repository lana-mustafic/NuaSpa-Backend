namespace NuaSpa.Application.Interfaces;

public sealed class IssuedRefreshToken
{
    public string Token { get; init; } = null!;

    public DateTime ExpiresAtUtc { get; init; }
}

public sealed class RefreshTokenRotationResult
{
    public bool Success { get; init; }

    public bool ReuseDetected { get; init; }

    public int? UserId { get; init; }

    public IssuedRefreshToken? RefreshToken { get; init; }
}

public interface IRefreshTokenService
{
    Task<IssuedRefreshToken> IssueAsync(int userId, Guid? familyId = null, CancellationToken ct = default);

    Task<RefreshTokenRotationResult> RotateAsync(string plainRefreshToken, CancellationToken ct = default);

    Task RevokeByPlainTokenAsync(string? plainRefreshToken, CancellationToken ct = default);

    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}
