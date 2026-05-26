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
                $"Trajanje usluge ({duration} min) prelazi maksimalno dozvoljeno ({MaxDurationMinutes} min).");
        }

        return duration;
    }

    public static decimal ResolveChargeAmount(decimal catalogPrice, decimal? snapshotPrice = null)
    {
        var amount = snapshotPrice is > 0 ? snapshotPrice.Value : catalogPrice;
        if (amount < 0)
        {
            throw new BusinessRuleException("Cijena rezervacije ne može biti negativna.");
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
            throw new BusinessRuleException("Spa centar je zatvoren za odabrani dan.");
        }

        var startMin = slotStart.Hour * 60 + slotStart.Minute;
        var endMin = startMin + durationMinutes;

        if (startMin < openMin)
        {
            throw new BusinessRuleException("Termin počinje prije radnog vremena spa centra.");
        }

        if (endMin > closeMin)
        {
            throw new BusinessRuleException(
                "Cijeli termin (uključujući trajanje usluge) mora stati unutar radnog vremena spa centra.");
        }
    }
}
