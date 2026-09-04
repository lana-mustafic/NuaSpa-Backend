using System;
using System.Linq;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services.Booking;

/// <summary>
/// Interval overlap used by both resource preview and final booking validation.
/// Two ranges overlap when A.start &lt; B.end and B.start &lt; A.end.
/// </summary>
public static class BookingOverlap
{
    public const int FallbackDurationMinutes = 60;

    public static IQueryable<Rezervacija> WhereOverlapping(
        this IQueryable<Rezervacija> query,
        DateTime start,
        int durationMinutes,
        int? excludeRezervacijaId = null)
    {
        var utcStart = BookingClock.ToUtc(start);
        var duration = durationMinutes > 0 ? durationMinutes : FallbackDurationMinutes;
        var utcEnd = utcStart.AddMinutes(duration);

        return query.Where(r =>
            r.Status != RezervacijaStatus.Cancelled &&
            (!excludeRezervacijaId.HasValue || r.Id != excludeRezervacijaId.Value) &&
            r.DatumRezervacije < utcEnd &&
            r.DatumRezervacije.AddMinutes(
                r.SnimakTrajanjeMinuta > 0
                    ? r.SnimakTrajanjeMinuta
                    : FallbackDurationMinutes) > utcStart);
    }
}
