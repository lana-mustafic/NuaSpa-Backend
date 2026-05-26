using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services
{
    public class FavoritService : IFavoritService
    {
        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public FavoritService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResult<UslugaDTO>> GetMyFavoritesAsync(
            int korisnikId,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize)
        {
            (page, pageSize) = PaginationHelper.Normalize(page, pageSize);

            var query = _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Where(f => !f.Usluga.IsDeleted)
                .OrderByDescending(f => f.CreatedAt);

            var total = await query.CountAsync();
            var favoriti = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(f => f.Usluga)
                    .ThenInclude(u => u.KategorijaUsluga)
                .ToListAsync();

            return new PagedResult<UslugaDTO>
            {
                Ukupno = total,
                Stranica = page,
                VelicinaStranice = pageSize,
                Items = _mapper.Map<IReadOnlyList<UslugaDTO>>(
                    favoriti.Select(f => f.Usluga).ToList()),
            };
        }

        public async Task<HashSet<int>> GetMyFavoriteIdsAsync(int korisnikId)
        {
            var ids = await _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.UslugaId)
                .Take(PaginationConstants.MaxPageSize)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task<bool> AddAsync(int korisnikId, int uslugaId)
        {
            var exists = await _context.Favoriti
                .AnyAsync(f => f.KorisnikId == korisnikId && f.UslugaId == uslugaId);

            if (exists) return true;

            var entity = new Favorit
            {
                KorisnikId = korisnikId,
                UslugaId = uslugaId,
                CreatedAt = System.DateTime.Now,
                IsDeleted = false
            };

            _context.Favoriti.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(int korisnikId, int uslugaId)
        {
            var entity = await _context.Favoriti
                .FirstOrDefaultAsync(f => f.KorisnikId == korisnikId && f.UslugaId == uslugaId);

            if (entity == null) return true;

            _context.Favoriti.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
