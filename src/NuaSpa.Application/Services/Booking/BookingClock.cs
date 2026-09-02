using System;
using NuaSpa.Application.Exceptions;

namespace NuaSpa.Application.Services.Booking;

/// <summary>
/// UTC semantika za usporedbu termina (RS2). Unspecified se tretira kao UTC.
/// </summary>
public static class BookingClock
{
    public static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

    public static DateTime UtcDate(DateTime value) =>
        DateTime.SpecifyKind(ToUtc(value).Date, DateTimeKind.Utc);

    public static void EnsureStartNotInPast(DateTime start)
    {
        if (ToUtc(start) <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("Cannot book an appointment in the past.");
        }
    }
}
