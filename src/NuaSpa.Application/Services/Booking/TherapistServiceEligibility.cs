using System;
using System.Collections.Generic;
using System.Linq;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Services.Booking;

/// <summary>
/// Shared rules: therapist can perform a service when categories match and
/// the service name appears in the therapist's Specijalizacija list.
/// </summary>
public static class TherapistServiceEligibility
{
    public static List<string> ParseSpecNames(string raw) =>
        raw
            .Split(new[] { ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static bool Matches(
        Usluga usluga,
        Zaposlenik zaposlenik,
        bool requireActive = true)
    {
        if (requireActive && zaposlenik.Status != ZaposlenikStatus.Active)
        {
            return false;
        }

        if (zaposlenik.KategorijaUslugaId is not int katId || katId != usluga.KategorijaUslugaId)
        {
            return false;
        }

        var serviceName = usluga.Naziv.Trim();
        if (serviceName.Length == 0)
        {
            return false;
        }

        return ParseSpecNames(zaposlenik.Specijalizacija)
            .Any(n => string.Equals(n, serviceName, StringComparison.OrdinalIgnoreCase));
    }
}
