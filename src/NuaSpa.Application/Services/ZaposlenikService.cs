using AutoMapper;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Services
{
    public class ZaposlenikService : BaseService<ZaposlenikDTO, Zaposlenik, object>, IZaposlenikService
    {
        public ZaposlenikService(NuaSpaContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<ZaposlenikDTO> Insert(ZaposlenikDTO dto)
        {
            var entity = _mapper.Map<Zaposlenik>(dto);
            entity.DatumZaposlenja = DateTime.UtcNow;

            _context.Zaposlenici.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<ZaposlenikDTO>(entity);
        }
    }
}

