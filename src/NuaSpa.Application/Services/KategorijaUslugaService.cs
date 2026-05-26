using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
    public KategorijaUslugaService(NuaSpaContext context, IMapper mapper)
        : base(context, mapper)
    {
    }

    public override async Task<IEnumerable<KategorijaUslugaDTO>> Get(
        KategorijaUslugaSearchObject? search = null)
    {
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

        var list = await query.OrderBy(k => k.Naziv).ToListAsync();
        return _mapper.Map<IEnumerable<KategorijaUslugaDTO>>(list);
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
        return (true, null);
    }
}