using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services;

public class SessionEligibilityService : ISessionEligibilityService
{
    public const string TherapistNotEligibleMessage =
        "Your therapist profile is not active. Contact your spa administrator.";

    private readonly UserManager<Korisnik> _userManager;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly NuaSpaContext _context;

    public SessionEligibilityService(
        UserManager<Korisnik> userManager,
        IRefreshTokenService refreshTokenService,
        NuaSpaContext context)
    {
        _userManager = userManager;
        _refreshTokenService = refreshTokenService;
        _context = context;
    }

    public async Task<bool> IsEligibleForSessionAsync(Korisnik user, CancellationToken ct = default)
    {
        var isTherapist = user.ZaposlenikId is > 0
            || await _userManager.IsInRoleAsync(user, RoleConstants.Zaposlenik);
        if (!isTherapist)
        {
            return true;
        }

        if (user.ZaposlenikId is not int zaposlenikId || zaposlenikId <= 0)
        {
            return false;
        }

        var status = await _context.Zaposlenici.AsNoTracking()
            .Where(z => z.Id == zaposlenikId)
            .Select(z => (ZaposlenikStatus?)z.Status)
            .FirstOrDefaultAsync(ct);

        return status == ZaposlenikStatus.Active;
    }

    public async Task EnsureEligibleForSessionAsync(Korisnik user, CancellationToken ct = default)
    {
        if (!await IsEligibleForSessionAsync(user, ct))
        {
            throw new UnauthorizedException(TherapistNotEligibleMessage);
        }
    }

    public async Task CloseTherapistSessionsAsync(
        int zaposlenikId,
        bool unlinkAccount,
        CancellationToken ct = default)
    {
        var users = await _userManager.Users
            .Where(u => u.ZaposlenikId == zaposlenikId)
            .ToListAsync(ct);

        foreach (var user in users)
        {
            await _refreshTokenService.RevokeAllForUserAsync(user.Id, ct);

            if (!unlinkAccount)
            {
                continue;
            }

            if (await _userManager.IsInRoleAsync(user, RoleConstants.Zaposlenik))
            {
                await _userManager.RemoveFromRoleAsync(user, RoleConstants.Zaposlenik);
            }

            user.ZaposlenikId = null;
            var remainingRoles = await _userManager.GetRolesAsync(user);
            if (remainingRoles.Count == 0)
            {
                user.Status = false;
            }

            await _userManager.UpdateAsync(user);
        }

        var pendingInvites = await _context.StaffInvitations
            .Where(i => i.ZaposlenikId == zaposlenikId && i.AcceptedAt == null)
            .ToListAsync(ct);

        if (pendingInvites.Count == 0)
        {
            return;
        }

        var expired = DateTime.UtcNow.AddSeconds(-1);
        foreach (var invite in pendingInvites)
        {
            invite.ExpiresAt = expired;
        }

        await _context.SaveChangesAsync(ct);
    }
}
