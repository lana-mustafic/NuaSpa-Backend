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
    }
}

