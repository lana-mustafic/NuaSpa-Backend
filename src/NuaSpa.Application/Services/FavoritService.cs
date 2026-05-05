using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<UslugaDTO>> GetMyFavoritesAsync(int korisnikId)
        {
            var usluge = await _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Include(f => f.Usluga)
                    .ThenInclude(u => u.KategorijaUsluga)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Usluga)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UslugaDTO>>(usluge);
        }

        public async Task<HashSet<int>> GetMyFavoriteIdsAsync(int korisnikId)
        {
            var ids = await _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Select(f => f.UslugaId)
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

