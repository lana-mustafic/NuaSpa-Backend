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
    public class RecenzijaService : IRecenzijaService
    {
        private readonly NuaSpaContext _context;
        private readonly IMapper _mapper;

        public RecenzijaService(NuaSpaContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RecenzijaDTO>> GetByUslugaAsync(int uslugaId)
        {
            var list = await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .Where(r => r.UslugaId == uslugaId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RecenzijaDTO>>(list);
        }

        public async Task<RecenzijaDTO> CreateAsync(int korisnikId, RecenzijaCreateDTO dto)
        {
            var entity = new Recenzija
            {
                KorisnikId = korisnikId,
                UslugaId = dto.UslugaId,
                Ocjena = dto.Ocjena,
                Komentar = dto.Komentar,
                CreatedAt = System.DateTime.Now,
                IsDeleted = false
            };

            _context.Recenzije.Add(entity);
            await _context.SaveChangesAsync();

            var created = await _context.Recenzije
                .AsNoTracking()
                .Include(r => r.Korisnik)
                .Include(r => r.Usluga)
                .FirstAsync(r => r.Id == entity.Id);

            return _mapper.Map<RecenzijaDTO>(created);
        }
    }
}

