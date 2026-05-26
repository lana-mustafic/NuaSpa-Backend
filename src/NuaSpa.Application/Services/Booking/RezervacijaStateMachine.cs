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

    public static void SyncLegacyFlags(Rezervacija entity)
    {
        switch (entity.Status)
        {
            case RezervacijaStatus.Pending:
                entity.IsPotvrdjena = false;
                entity.IsOtkazana = false;
                break;
            case RezervacijaStatus.Confirmed:
                entity.IsPotvrdjena = true;
                entity.IsOtkazana = false;
                break;
            case RezervacijaStatus.Cancelled:
                entity.IsPotvrdjena = false;
                entity.IsOtkazana = true;
                break;
            case RezervacijaStatus.Completed:
                entity.IsPotvrdjena = true;
                entity.IsOtkazana = false;
                break;
        }
    }

    public static RezervacijaStatus ResolveTarget(bool isPotvrdjena, bool isOtkazana)
    {
        if (isOtkazana) return RezervacijaStatus.Cancelled;
        if (isPotvrdjena) return RezervacijaStatus.Confirmed;
        return RezervacijaStatus.Pending;
    }
}
