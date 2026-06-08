using System.Linq;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Common;

/// <summary>
/// Shared review query helpers (therapist attribution for legacy rows without ZaposlenikId).
/// </summary>
public static class RecenzijaQueryExtensions
{
    public static IQueryable<Recenzija> ForTherapist(
        this IQueryable<Recenzija> query,
        NuaSpaContext context,
        int zaposlenikId)
    {
        return query.Where(rev =>
            rev.ZaposlenikId == zaposlenikId
            || (rev.ZaposlenikId == null && context.Rezervacije.Any(rez =>
                rez.ZaposlenikId == zaposlenikId
                && rez.KorisnikId == rev.KorisnikId
                && rez.UslugaId == rev.UslugaId
                && rez.Status == RezervacijaStatus.Completed
                && !rez.IsOtkazana
                && !rez.IsDeleted)));
    }
}
