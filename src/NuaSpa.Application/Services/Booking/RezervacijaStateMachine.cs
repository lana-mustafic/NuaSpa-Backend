using NuaSpa.Application.Exceptions;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services.Booking;

/// <summary>Centralizirana logika dozvoljenih prelaza statusa rezervacije.</summary>
public static class RezervacijaStateMachine
{
    private static readonly IReadOnlyDictionary<RezervacijaStatus, RezervacijaStatus[]> Allowed =
        new Dictionary<RezervacijaStatus, RezervacijaStatus[]>
        {
            [RezervacijaStatus.Pending] = [RezervacijaStatus.Confirmed, RezervacijaStatus.Cancelled],
            [RezervacijaStatus.Confirmed] = [RezervacijaStatus.Cancelled, RezervacijaStatus.Completed],
            [RezervacijaStatus.Cancelled] = [],
            [RezervacijaStatus.Completed] = [],
        };

    public static void EnsureTransition(RezervacijaStatus from, RezervacijaStatus to)
    {
        if (from == to)
        {
            return;
        }

        if (!Allowed.TryGetValue(from, out var targets) || !targets.Contains(to))
        {
            throw new BusinessRuleException(
                $"Prelaz statusa '{from}' → '{to}' nije dozvoljen.");
        }
    }

    public static void ApplyStatus(Rezervacija entity, RezervacijaStatus newStatus)
    {
        entity.Status = newStatus;
        SyncLegacyFlags(entity);
    }

    public static void SyncLegacyFlags(Rezervacija entity) =>
        entity.SyncLegacyFlagsFromStatus();
}
