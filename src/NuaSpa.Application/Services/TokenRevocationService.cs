using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class TokenRevocationService : ITokenRevocationService
{
    private readonly NuaSpaContext _context;

    public TokenRevocationService(NuaSpaContext context)
    {
        _context = context;
    }

    public async Task RevokeAsync(string jti, DateTime expiresAtUtc, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return;
        }

        var exists = await _context.RevokedJwts.AsNoTracking()
            .AnyAsync(x => x.Jti == jti, ct);
        if (exists)
        {
            return;
        }

        _context.RevokedJwts.Add(new RevokedJwt
        {
            Jti = jti,
            ExpiresAtUtc = expiresAtUtc.ToUniversalTime(),
            RevokedAtUtc = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        return await _context.RevokedJwts.AsNoTracking()
            .AnyAsync(x => x.Jti == jti && x.ExpiresAtUtc > DateTime.UtcNow, ct);
    }
}
