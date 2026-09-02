using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Interfaces;

/// <summary>
/// Shared rule for issuing a new access/refresh session, plus closing
/// therapist portal access when a profile is deactivated or deleted.
/// </summary>
public interface ISessionEligibilityService
{
    Task<bool> IsEligibleForSessionAsync(Korisnik user, CancellationToken ct = default);

    Task EnsureEligibleForSessionAsync(Korisnik user, CancellationToken ct = default);

    /// <summary>
    /// Revokes refresh tokens for accounts linked to this therapist and expires
    /// pending invites. When <paramref name="unlinkAccount"/> is true, also
    /// removes the therapist role, clears <c>ZaposlenikId</c>, and deactivates
    /// accounts that have no remaining roles.
    /// </summary>
    Task CloseTherapistSessionsAsync(
        int zaposlenikId,
        bool unlinkAccount,
        CancellationToken ct = default);
}
