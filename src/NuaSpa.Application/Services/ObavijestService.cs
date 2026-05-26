using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Exceptions;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services;

public class ObavijestService : IObavijestService
{
    private readonly NuaSpaContext _context;

    public ObavijestService(NuaSpaContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ObavijestDto>> GetPublishedAsync(
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);
        var query = _context.Obavijesti
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.Aktivna)
            .OrderByDescending(o => o.DatumObjave);

        return await ToPagedDtoAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<ObavijestDto>> GetAllAdminAsync(
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        (page, pageSize) = PaginationHelper.Normalize(page, pageSize);
        var query = _context.Obavijesti
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.DatumObjave);

        return await ToPagedDtoAsync(query, page, pageSize, ct);
    }

    private async Task<PagedResult<ObavijestDto>> ToPagedDtoAsync(
        IQueryable<Obavijest> query,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(Map)
            .ToListAsync(ct);

        return new PagedResult<ObavijestDto>
        {
            Ukupno = total,
            Stranica = page,
            VelicinaStranice = pageSize,
            Items = items,
        };
    }

    public async Task<ObavijestDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Obavijesti
            .AsNoTracking()
            .Where(o => o.Id == id && !o.IsDeleted)
            .Select(Map)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ObavijestDto> CreateAsync(ObavijestCreateDto dto, CancellationToken ct = default)
    {
        Validate(dto.Naslov, dto.Tekst);

        var entity = new Obavijest
        {
            Naslov = dto.Naslov.Trim(),
            Tekst = dto.Tekst.Trim(),
            SlikaUrl = string.IsNullOrWhiteSpace(dto.SlikaUrl) ? null : dto.SlikaUrl.Trim(),
            DatumObjave = dto.DatumObjave ?? DateTime.UtcNow,
            Aktivna = dto.Aktivna,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Obavijesti.Add(entity);
        await _context.SaveChangesAsync(ct);

        return MapEntity(entity);
    }

    public async Task<ObavijestDto?> UpdateAsync(int id, ObavijestUpdateDto dto, CancellationToken ct = default)
    {
        Validate(dto.Naslov, dto.Tekst);

        var entity = await _context.Obavijesti.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
        if (entity == null)
        {
            return null;
        }

        entity.Naslov = dto.Naslov.Trim();
        entity.Tekst = dto.Tekst.Trim();
        entity.SlikaUrl = string.IsNullOrWhiteSpace(dto.SlikaUrl) ? null : dto.SlikaUrl.Trim();
        entity.DatumObjave = dto.DatumObjave ?? entity.DatumObjave;
        entity.Aktivna = dto.Aktivna;

        await _context.SaveChangesAsync(ct);
        return MapEntity(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Obavijesti.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static void Validate(string naslov, string tekst)
    {
        if (string.IsNullOrWhiteSpace(naslov))
        {
            throw new BusinessRuleException("Naslov obavijesti je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(tekst))
        {
            throw new BusinessRuleException("Tekst obavijesti je obavezan.");
        }
    }

    private static ObavijestDto MapEntity(Obavijest o) => new()
    {
        Id = o.Id,
        Naslov = o.Naslov,
        Tekst = o.Tekst,
        SlikaUrl = o.SlikaUrl,
        DatumObjave = o.DatumObjave,
        Aktivna = o.Aktivna,
    };

    private static System.Linq.Expressions.Expression<Func<Obavijest, ObavijestDto>> Map =>
        o => new ObavijestDto
        {
            Id = o.Id,
            Naslov = o.Naslov,
            Tekst = o.Tekst,
            SlikaUrl = o.SlikaUrl,
            DatumObjave = o.DatumObjave,
            Aktivna = o.Aktivna,
        };
}
