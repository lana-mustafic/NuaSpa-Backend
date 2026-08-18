using NuaSpa.Application.Exceptions;

namespace NuaSpa.Application.Services.Booking;

/// <summary>Izračun cijene i validacija trajanja termina (uključujući duge usluge).</summary>
public static class RezervacijaPricing
{
    public const int MaxDurationMinutes = 72 * 60;

    public static int ResolveDurationMinutes(int catalogMinutes, int? snapshotMinutes = null)
    {
        var duration = snapshotMinutes is > 0 ? snapshotMinutes.Value : catalogMinutes;
        if (duration <= 0)
        {
            duration = 60;
        }

        if (duration > MaxDurationMinutes)
        {
            throw new BusinessRuleException(
                $"Service duration ({duration} min) exceeds the maximum allowed ({MaxDurationMinutes} min).");
        }

        return duration;
    }

    public static decimal ResolveChargeAmount(decimal catalogPrice, decimal? snapshotPrice = null)
    {
        var amount = snapshotPrice is > 0 ? snapshotPrice.Value : catalogPrice;
        if (amount < 0)
        {
            throw new BusinessRuleException("Booking price cannot be negative.");
        }

        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static long ToStripeMinorUnits(decimal amountKm)
    {
        return (long)decimal.Round(amountKm * 100m, 0, MidpointRounding.AwayFromZero);
    }

    public static void ValidateFitsWorkingHours(
        DateTime slotStart,
        int durationMinutes,
        bool isClosed,
        int openMin,
        int closeMin)
    {
        if (isClosed)
        {
            throw new BusinessRuleException("The spa is closed on the selected day.");
        }

        var startMin = slotStart.Hour * 60 + slotStart.Minute;
        var endMin = startMin + durationMinutes;

        if (startMin < openMin)
        {
            throw new BusinessRuleException("The appointment starts before the spa opens.");
        }

        if (endMin > closeMin)
        {
            throw new BusinessRuleException(
                "The full appointment (including service duration) must fit within spa working hours.");
        }
    }
}
