using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class KategorijaUslugaService
    : BaseService<KategorijaUslugaDTO, KategorijaUsluga, KategorijaUslugaSearchObject>,
        IKategorijaUslugaService
{
    private const string ActiveCategoriesCacheKey = "kategorije:active";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _cache;

    public KategorijaUslugaService(NuaSpaContext context, IMapper mapper, IMemoryCache cache)
        : base(context, mapper)
    {
        _cache = cache;
    }

    public override async Task<PagedResult<KategorijaUslugaDTO>> Get(
        KategorijaUslugaSearchObject? search = null)
    {
        var hasFilter = search?.IncludeDeleted == true
            || !string.IsNullOrWhiteSpace(search?.Naziv);

        if (!hasFilter)
        {
            var (page, pageSize) = PaginationHelper.FromSearch(search);
            var cached = await _cache.GetOrCreateAsync(ActiveCategoriesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                var all = await _context.KategorijeUsluga.AsNoTracking()
                    .Where(k => !k.IsDeleted)
                    .OrderBy(k => k.Naziv)
                    .ToListAsync();
                return _mapper.Map<List<KategorijaUslugaDTO>>(all);
            }) ?? new List<KategorijaUslugaDTO>();

            return PaginateInMemory(cached, page, pageSize);
        }

        var (p, ps) = PaginationHelper.FromSearch(search);
        var query = _context.KategorijeUsluga.AsNoTracking().AsQueryable();

        if (search?.IncludeDeleted != true)
        {
            query = query.Where(k => !k.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search?.Naziv))
        {
            var naziv = search.Naziv.Trim();
            query = query.Where(k => k.Naziv.Contains(naziv));
        }

        var paged = await PaginationHelper.ToPagedAsync(
            query.OrderBy(k => k.Naziv),
            p,
            ps);

        return new PagedResult<KategorijaUslugaDTO>
        {
            Ukupno = paged.Ukupno,
            Stranica = paged.Stranica,
            VelicinaStranice = paged.VelicinaStranice,
            Items = _mapper.Map<IReadOnlyList<KategorijaUslugaDTO>>(paged.Items),
        };
    }

    private static PagedResult<KategorijaUslugaDTO> PaginateInMemory(
        List<KategorijaUslugaDTO> all,
        int page,
        int pageSize)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);
        var total = all.Count;
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<KategorijaUslugaDTO>
        {
            Ukupno = total,
            Stranica = page,
            VelicinaStranice = pageSize,
            Items = items,
        };
    }

    public override async Task<KategorijaUslugaDTO> Insert(KategorijaUslugaDTO dto)
    {
        var created = await base.Insert(dto);
        _cache.Remove(ActiveCategoriesCacheKey);
        return created;
    }

    public async Task<KategorijaUslugaDTO> UpdateAsync(KategorijaUslugaDTO dto)
    {
        var entity = await _context.Set<KategorijaUsluga>().FindAsync(dto.Id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Kategorija id={dto.Id} ne postoji.");
        }

        entity.Naziv = dto.Naziv;
        await _context.SaveChangesAsync();
        _cache.Remove(ActiveCategoriesCacheKey);
        return _mapper.Map<KategorijaUslugaDTO>(entity);
    }

    public async Task<(bool Ok, string? Message)> DeleteAsync(int id)
    {
        var entity = await _context.Set<KategorijaUsluga>().FindAsync(id);
        if (entity == null || entity.IsDeleted)
        {
            return (false, "Kategorija ne postoji.");
        }

        if (await _context.Usluge.AsNoTracking()
                .AnyAsync(u => u.KategorijaUslugaId == id && !u.IsDeleted))
        {
            return (false, "Kategorija ima pridružene usluge i ne može se obrisati. Prvo uklonite ili arhivirajte usluge.");
        }

        if (await _context.Zaposlenici.AsNoTracking()
                .AnyAsync(z => z.KategorijaUslugaId == id && !z.IsDeleted))
        {
            return (false, "Kategoriju koriste zaposlenici i ne može se obrisati.");
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();
        _cache.Remove(ActiveCategoriesCacheKey);
        return (true, null);
    }
}