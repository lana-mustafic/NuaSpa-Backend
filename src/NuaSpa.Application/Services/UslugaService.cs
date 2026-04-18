using AutoMapper;
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
    }
}