using NuaSpa.Domain.Enums;

namespace NuaSpa.Domain.Common;

/// <summary>
/// <see cref="RezervacijaStatus"/> is the single source of truth for booking lifecycle.
/// Legacy <c>IsPotvrdjena</c> / <c>IsOtkazana</c> columns are derived from it.
/// </summary>
public static class RezervacijaStatusRules
{
    public static bool IsCancelled(RezervacijaStatus status) =>
        status == RezervacijaStatus.Cancelled;

    public static bool OccupiesSlot(RezervacijaStatus status) =>
        status != RezervacijaStatus.Cancelled;

    public static bool IsConfirmedLike(RezervacijaStatus status) =>
        status is RezervacijaStatus.Confirmed or RezervacijaStatus.Completed;

    public static bool LegacyIsOtkazana(RezervacijaStatus status) =>
        IsCancelled(status);

    public static bool LegacyIsPotvrdjena(RezervacijaStatus status) =>
        IsConfirmedLike(status);
}
