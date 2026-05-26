using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;

namespace NuaSpa.Application.Services;

public class LookupService : ILookupService
{
    private readonly NuaSpaContext _context;

    public LookupService(NuaSpaContext context)
    {
        _context = context;
    }

    public async Task<List<DrzavaLookupDto>> GetDrzaveAsync(string? naziv, CancellationToken ct)
    {
        var query = _context.Drzave.AsNoTracking().Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            query = query.Where(d => d.Naziv.Contains(t));
        }

        return await query
            .OrderBy(d => d.Naziv)
            .Select(d => new DrzavaLookupDto { Id = d.Id, Naziv = d.Naziv })
            .ToListAsync(ct);
    }

    public async Task<List<GradLookupDto>> GetGradoviAsync(int? drzavaId, string? naziv, CancellationToken ct)
    {
        var query = _context.Gradovi.AsNoTracking()
            .Include(g => g.Drzava)
            .Where(g => !g.IsDeleted);

        if (drzavaId is > 0)
        {
            query = query.Where(g => g.DrzavaId == drzavaId);
        }

        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            query = query.Where(g =>
                g.Naziv.Contains(t) || g.PostanskiBroj.Contains(t));
        }

        return await query
            .OrderBy(g => g.Naziv)
            .Select(g => new GradLookupDto
            {
                Id = g.Id,
                Naziv = g.Naziv,
                PostanskiBroj = g.PostanskiBroj,
                DrzavaId = g.DrzavaId,
                DrzavaNaziv = g.Drzava.Naziv,
            })
            .ToListAsync(ct);
    }
}

