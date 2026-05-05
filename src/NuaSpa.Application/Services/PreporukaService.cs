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
    public class PreporukaService : IPreporukaService
    {
        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public PreporukaService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UslugaDTO>> GetForKorisnikAsync(int korisnikId, int take = 10)
        {
            var favCatIds = await _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Join(_context.Usluge, f => f.UslugaId, u => u.Id, (f, u) => u.KategorijaUslugaId)
                .Distinct()
                .ToListAsync();

            var rezCatIds = await _context.Rezervacije
                .AsNoTracking()
                .Where(r => r.KorisnikId == korisnikId)
                .Join(_context.Usluge, r => r.UslugaId, u => u.Id, (r, u) => u.KategorijaUslugaId)
                .Distinct()
                .ToListAsync();

            var categoryIds = favCatIds.Union(rezCatIds).Distinct().ToHashSet();

            var favUslugaIds = await _context.Favoriti
                .AsNoTracking()
                .Where(f => f.KorisnikId == korisnikId)
                .Select(f => f.UslugaId)
                .ToListAsync();
            var favSet = favUslugaIds.ToHashSet();

            async Task<List<Usluga>> QueryByCategories(bool excludeFavorites)
            {
                var q = _context.Usluge
                    .AsNoTracking()
                    .Include(u => u.KategorijaUsluga)
                    .Where(u => categoryIds.Contains(u.KategorijaUslugaId));

                if (excludeFavorites && favSet.Count > 0)
                {
                    q = q.Where(u => !favSet.Contains(u.Id));
                }

                return await q
                    .OrderBy(u => u.Naziv)
                    .Take(take)
                    .ToListAsync();
            }

            List<Usluga> list;

            if (categoryIds.Count > 0)
            {
                list = await QueryByCategories(excludeFavorites: true);
                if (list.Count == 0)
                {
                    list = await QueryByCategories(excludeFavorites: false);
                }
            }
            else
            {
                list = await _context.Usluge
                    .AsNoTracking()
                    .Include(u => u.KategorijaUsluga)
                    .OrderBy(u => u.Naziv)
                    .Take(take)
                    .ToListAsync();
            }

            return _mapper.Map<IEnumerable<UslugaDTO>>(list);
        }
    }
}
