using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuaSpa.Application.Common;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly NuaSpaContext _context;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(NuaSpaContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<IssuedRefreshToken> IssueAsync(
        int userId,
        Guid? familyId = null,
        CancellationToken ct = default)
    {
        var plain = GeneratePlainToken();
        var hash = HashToken(plain);
        var now = DateTime.UtcNow;
        var expires = now.AddDays(Math.Max(1, _jwtSettings.RefreshTokenDurationDays));

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            FamilyId = familyId ?? Guid.NewGuid(),
            CreatedAtUtc = now,
            ExpiresAtUtc = expires,
        };

        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync(ct);

        return new IssuedRefreshToken
        {
            Token = plain,
            ExpiresAtUtc = expires,
        };
    }

    public async Task<RefreshTokenRotationResult> RotateAsync(
        string plainRefreshToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plainRefreshToken))
        {
            return new RefreshTokenRotationResult { Success = false };
        }

        var hash = HashToken(plainRefreshToken);
        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (existing == null)
        {
            return new RefreshTokenRotationResult { Success = false };
        }

        if (existing.RevokedAtUtc.HasValue)
        {
            await RevokeFamilyAsync(existing.FamilyId, ct);
            return new RefreshTokenRotationResult
            {
                Success = false,
                ReuseDetected = true,
            };
        }

        if (existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return new RefreshTokenRotationResult { Success = false };
        }

        var replacement = await IssueAsync(existing.UserId, existing.FamilyId, ct);
        var replacementEntity = await _context.RefreshTokens.AsNoTracking()
            .Where(x => x.TokenHash == HashToken(replacement.Token))
            .Select(x => new { x.Id })
            .FirstAsync(ct);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedById = replacementEntity.Id;
        await _context.SaveChangesAsync(ct);

        return new RefreshTokenRotationResult
        {
            Success = true,
            UserId = existing.UserId,
            RefreshToken = replacement,
        };
    }

    public async Task RevokeByPlainTokenAsync(string? plainRefreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(plainRefreshToken))
        {
            return;
        }

        var hash = HashToken(plainRefreshToken);
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (token == null || token.RevokedAtUtc.HasValue)
        {
            return;
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var active = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var token in active)
        {
            token.RevokedAtUtc = now;
        }

        if (active.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var familyTokens = await _context.RefreshTokens
            .Where(x => x.FamilyId == familyId && x.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var token in familyTokens)
        {
            token.RevokedAtUtc = now;
        }

        if (familyTokens.Count > 0)
        {
            await _context.SaveChangesAsync(ct);
        }
    }

    private static string GeneratePlainToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    internal static string HashToken(string plainToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToBase64String(hash);
    }
}
