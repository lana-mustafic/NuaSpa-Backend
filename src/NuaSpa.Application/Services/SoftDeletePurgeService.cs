using Microsoft.EntityFrameworkCore;
using NuaSpa.Domain;

namespace NuaSpa.Application.Services;

/// <summary>
/// Trajno uklanja soft-deleted zapise u redoslijedu koji poštuje FK (djeca prije roditelja).
/// </summary>
public sealed class SoftDeletePurgeService
{
    private readonly NuaSpaContext _context;

    public SoftDeletePurgeService(NuaSpaContext context)
    {
        _context = context;
    }

    public async Task<int> PurgeSoftDeletedAsync(CancellationToken cancellationToken = default)
    {
        var total = 0;

        total += await _context.KorisnikAktivnosti.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Favoriti.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.RezervacijeOprema.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Placanja.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Recenzije.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Rezervacije.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.NarudzbeProizvoda.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Skladista.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Usluge.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.Proizvodi.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        total += await _context.KategorijeUsluga.IgnoreQueryFilters()
            .Where(x => x.IsDeleted).ExecuteDeleteAsync(cancellationToken);

        return total;
    }
}
