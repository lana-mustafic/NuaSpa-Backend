using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

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

    public async Task<PagedResult<DrzavaLookupDto>> GetDrzaveAsync(
        string? naziv,
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        if (!string.IsNullOrWhiteSpace(naziv))
        {
            var t = naziv.Trim();
            var query = _context.Drzave.AsNoTracking()
                .Where(d => !d.IsDeleted && d.Naziv.Contains(t))
                .OrderBy(d => d.Naziv)
                .Select(ToDrzavaDto);

            return await PaginationHelper.ToPagedAsync(query, page, pageSize, ct);
        }

        var cached = await _cache.GetOrCreateAsync(DrzaveCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _context.Drzave.AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Naziv)
                .Select(ToDrzavaDto)
                .ToListAsync(ct);
        }) ?? new List<DrzavaLookupDto>();

        return PaginateInMemory(cached, page, pageSize);
    }

    public async Task<PagedResult<GradLookupDto>> GetGradoviAsync(
        int? drzavaId,
        string? naziv,
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

        var query = _context.Gradovi.AsNoTracking()
            .Include(g => g.Drzava)
            .Where(g => !g.IsDeleted && !g.Drzava.IsDeleted);

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

        var projected = query
            .OrderBy(g => g.Naziv)
            .Select(g => new GradLookupDto
            {
                Id = g.Id,
                Naziv = g.Naziv,
                PostanskiBroj = g.PostanskiBroj,
                DrzavaId = g.DrzavaId,
                DrzavaNaziv = g.Drzava.Naziv,
            });

        return await PaginationHelper.ToPagedAsync(projected, page, pageSize, ct);
    }

    public async Task<DrzavaLookupDto> CreateDrzavaAsync(DrzavaWriteDto dto, CancellationToken ct)
    {
        var naziv = RequireName(dto.Naziv, "Naziv države je obavezan.");
        var pozivni = RequireCallingCode(dto.PozivniBroj);

        await EnsureUniqueDrzavaNameAsync(naziv, excludeId: null, ct);

        var entity = new Drzava
        {
            Naziv = naziv,
            PozivniBroj = pozivni,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Drzave.Add(entity);
        await _context.SaveChangesAsync(ct);
        InvalidateDrzaveCache();

        return MapDrzava(entity);
    }

    public async Task<DrzavaLookupDto> UpdateDrzavaAsync(int id, DrzavaWriteDto dto, CancellationToken ct)
    {
        var entity = await _context.Drzave.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct)
            ?? throw new NotFoundException("Država nije pronađena.");

        var naziv = RequireName(dto.Naziv, "Naziv države je obavezan.");
        var pozivni = RequireCallingCode(dto.PozivniBroj);
        await EnsureUniqueDrzavaNameAsync(naziv, excludeId: id, ct);

        entity.Naziv = naziv;
        entity.PozivniBroj = pozivni;
        await _context.SaveChangesAsync(ct);
        InvalidateDrzaveCache();

        return MapDrzava(entity);
    }

    public async Task DeleteDrzavaAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Drzave.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct)
            ?? throw new NotFoundException("Država nije pronađena.");

        var hasCities = await _context.Gradovi.AsNoTracking()
            .AnyAsync(g => g.DrzavaId == id && !g.IsDeleted, ct);
        if (hasCities)
        {
            throw new ConflictException(
                "Država ima gradove i ne može biti obrisana. Prvo uklonite ili premjestite gradove.");
        }

        var cityIds = await _context.Gradovi.AsNoTracking()
            .Where(g => g.DrzavaId == id)
            .Select(g => g.Id)
            .ToListAsync(ct);
        if (cityIds.Count > 0)
        {
            var used = await _context.Users.AsNoTracking()
                .AnyAsync(u => cityIds.Contains(u.GradId), ct);
            if (used)
            {
                throw new ConflictException(
                    "Državu koriste korisnički profili i ne može biti obrisana.");
            }
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
        InvalidateDrzaveCache();
    }

    public async Task<GradLookupDto> CreateGradAsync(GradWriteDto dto, CancellationToken ct)
    {
        var naziv = RequireName(dto.Naziv, "Naziv grada je obavezan.");
        var postanski = RequirePostalCode(dto.PostanskiBroj);
        await EnsureDrzavaExistsAsync(dto.DrzavaId, ct);
        await EnsureUniqueGradNameAsync(dto.DrzavaId, naziv, excludeId: null, ct);

        var entity = new Grad
        {
            Naziv = naziv,
            PostanskiBroj = postanski,
            DrzavaId = dto.DrzavaId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.Gradovi.Add(entity);
        await _context.SaveChangesAsync(ct);

        return await MapGradAsync(entity.Id, ct);
    }

    public async Task<GradLookupDto> UpdateGradAsync(int id, GradWriteDto dto, CancellationToken ct)
    {
        var entity = await _context.Gradovi.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, ct)
            ?? throw new NotFoundException("Grad nije pronađen.");

        var naziv = RequireName(dto.Naziv, "Naziv grada je obavezan.");
        var postanski = RequirePostalCode(dto.PostanskiBroj);
        await EnsureDrzavaExistsAsync(dto.DrzavaId, ct);
        await EnsureUniqueGradNameAsync(dto.DrzavaId, naziv, excludeId: id, ct);

        entity.Naziv = naziv;
        entity.PostanskiBroj = postanski;
        entity.DrzavaId = dto.DrzavaId;
        await _context.SaveChangesAsync(ct);

        return await MapGradAsync(entity.Id, ct);
    }

    public async Task DeleteGradAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Gradovi.FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, ct)
            ?? throw new NotFoundException("Grad nije pronađen.");

        var used = await _context.Users.AsNoTracking()
            .AnyAsync(u => u.GradId == id, ct);
        if (used)
        {
            throw new ConflictException(
                "Grad je već dodijeljen korisnicima i ne može biti obrisan. Promijenite grad na profilima prije brisanja.");
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
    }

    private async Task EnsureDrzavaExistsAsync(int drzavaId, CancellationToken ct)
    {
        var exists = await _context.Drzave.AsNoTracking()
            .AnyAsync(d => d.Id == drzavaId && !d.IsDeleted, ct);
        if (!exists)
        {
            throw new BusinessRuleException("Odabrana država nije pronađena.");
        }
    }

    private async Task EnsureUniqueDrzavaNameAsync(string naziv, int? excludeId, CancellationToken ct)
    {
        var taken = await _context.Drzave.AsNoTracking()
            .AnyAsync(d =>
                !d.IsDeleted
                && d.Naziv.ToLower() == naziv.ToLower()
                && (!excludeId.HasValue || d.Id != excludeId.Value), ct);
        if (taken)
        {
            throw new ConflictException("Država s tim nazivom već postoji.");
        }
    }

    private async Task EnsureUniqueGradNameAsync(int drzavaId, string naziv, int? excludeId, CancellationToken ct)
    {
        var taken = await _context.Gradovi.AsNoTracking()
            .AnyAsync(g =>
                !g.IsDeleted
                && g.DrzavaId == drzavaId
                && g.Naziv.ToLower() == naziv.ToLower()
                && (!excludeId.HasValue || g.Id != excludeId.Value), ct);
        if (taken)
        {
            throw new ConflictException("Grad s tim nazivom već postoji u odabranoj državi.");
        }
    }

    private async Task<GradLookupDto> MapGradAsync(int id, CancellationToken ct)
    {
        var dto = await _context.Gradovi.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GradLookupDto
            {
                Id = g.Id,
                Naziv = g.Naziv,
                PostanskiBroj = g.PostanskiBroj,
                DrzavaId = g.DrzavaId,
                DrzavaNaziv = g.Drzava.Naziv,
            })
            .FirstAsync(ct);
        return dto;
    }

    private static readonly System.Linq.Expressions.Expression<Func<Drzava, DrzavaLookupDto>> ToDrzavaDto =
        d => new DrzavaLookupDto
        {
            Id = d.Id,
            Naziv = d.Naziv,
            PozivniBroj = d.PozivniBroj,
        };

    private static DrzavaLookupDto MapDrzava(Drzava d) =>
        new() { Id = d.Id, Naziv = d.Naziv, PozivniBroj = d.PozivniBroj };

    private static string RequireName(string? value, string message)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new BusinessRuleException(message);
        }

        if (trimmed.Length > 100)
        {
            throw new BusinessRuleException("Naziv može imati najviše 100 znakova.");
        }

        return trimmed;
    }

    private static string RequireCallingCode(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new BusinessRuleException("Pozivni broj je obavezan.");
        }

        if (trimmed.Length > 10)
        {
            throw new BusinessRuleException("Pozivni broj može imati najviše 10 znakova.");
        }

        return trimmed;
    }

    private static string RequirePostalCode(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new BusinessRuleException("Poštanski broj je obavezan.");
        }

        if (trimmed.Length > 20)
        {
            throw new BusinessRuleException("Poštanski broj može imati najviše 20 znakova.");
        }

        return trimmed;
    }

    private void InvalidateDrzaveCache() => _cache.Remove(DrzaveCacheKey);

    private static PagedResult<T> PaginateInMemory<T>(
        List<T> all,
        int page,
        int pageSize)
    {
        var total = all.Count;
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Ukupno = total,
            Stranica = page,
            VelicinaStranice = pageSize,
            Items = items,
        };
    }
}
