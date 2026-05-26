namespace NuaSpa.Application.Interfaces;

public interface ITokenRevocationService
{
    Task RevokeAsync(string jti, DateTime expiresAtUtc, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}
