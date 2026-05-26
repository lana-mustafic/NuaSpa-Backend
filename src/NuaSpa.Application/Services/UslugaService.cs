using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services
{
    public class UslugaService : BaseService<UslugaDTO, Usluga, UslugaSearchObject>, IUslugaService
    {
        public UslugaService(NuaSpaContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<PagedResult<UslugaDTO>> Get(UslugaSearchObject? search = null)
        {
            var (page, pageSize) = PaginationHelper.FromSearch(search);
            var query = _context.Usluge
                .AsNoTracking()
                .Include(u => u.KategorijaUsluga)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search?.Naziv))
            {
                var naziv = search.Naziv.Trim();
                query = query.Where(u => u.Naziv.Contains(naziv));
            }

            if (search?.MaxCijena is decimal maxCijena)
            {
                query = query.Where(u => u.Cijena <= maxCijena);
            }

            var paged = await PaginationHelper.ToPagedAsync(
                query.OrderBy(u => u.Naziv),
                page,
                pageSize);

            return new PagedResult<UslugaDTO>
            {
                Ukupno = paged.Ukupno,
                Stranica = paged.Stranica,
                VelicinaStranice = paged.VelicinaStranice,
                Items = _mapper.Map<IReadOnlyList<UslugaDTO>>(paged.Items),
            };
        }

        public override async Task<UslugaDTO> GetById(int id)
        {
            var entity = await _context.Usluge
                .AsNoTracking()
                .Include(u => u.KategorijaUsluga)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Usluga sa id={id} ne postoji.");
            }

            return _mapper.Map<UslugaDTO>(entity);
        }

        public async Task<UslugaDTO> UpdateAsync(UslugaDTO dto)
        {
            var entity = await _context.Usluge.FindAsync(dto.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Usluga sa id={dto.Id} ne postoji.");
            }

            entity.Naziv = dto.Naziv;
            entity.Cijena = dto.Cijena;
            entity.TrajanjeMinuta = dto.TrajanjeMinuta;
            entity.Opis = dto.Opis;
            entity.KategorijaUslugaId = dto.KategorijaUslugaId;
            entity.SlikaUrl = dto.SlikaUrl;

            await _context.SaveChangesAsync();
            return await GetById(entity.Id);
        }

        public async Task<(bool Ok, string? Message)> DeleteAsync(int id)
        {
            var entity = await _context.Usluge.FindAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return (false, "Usluga ne postoji.");
            }

            if (await _context.Rezervacije.AsNoTracking()
                    .AnyAsync(r => r.UslugaId == id && !r.IsDeleted))
            {
                return (false, "Usluga ima aktivne rezervacije i ne može se obrisati. Prvo arhivirajte ili otkažite rezervacije.");
            }

            if (await _context.Favoriti.AsNoTracking()
                    .AnyAsync(f => f.UslugaId == id && !f.IsDeleted))
            {
                return (false, "Usluga je u favoritima korisnika i ne može se obrisati.");
            }

            if (await _context.Recenzije.AsNoTracking()
                    .AnyAsync(r => r.UslugaId == id && !r.IsDeleted))
            {
                return (false, "Usluga ima recenzije i ne može se obrisati.");
            }

            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}