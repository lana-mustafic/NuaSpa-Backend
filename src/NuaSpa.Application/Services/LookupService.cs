using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;

namespace NuaSpa.Application.Services;

public class LookupService : ILookupService
{
    private const string DrzaveCacheKey = "lookup:drzave:all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly NuaSpaContext _context;
    private readonly IMemoryCache _cache;

    public LookupService(NuaSpaContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<DrzavaLookupDto>> GetDrzaveAsync(string? naziv, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            return await _context.Drzave.AsNoTracking()
                .Where(d => !d.IsDeleted && d.Naziv.Contains(t))
                .OrderBy(d => d.Naziv)
                .Select(d => new DrzavaLookupDto { Id = d.Id, Naziv = d.Naziv })
                .ToListAsync(ct);
        }

        return await _cache.GetOrCreateAsync(DrzaveCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _context.Drzave.AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Naziv)
                .Select(d => new DrzavaLookupDto { Id = d.Id, Naziv = d.Naziv })
                .ToListAsync(ct);
        }) ?? new List<DrzavaLookupDto>();
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
            .Take(PaginationConstants.MaxPageSize)
            .ToListAsync(ct);
    }
}
