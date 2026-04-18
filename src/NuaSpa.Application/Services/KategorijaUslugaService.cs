using AutoMapper;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Services;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;

public class KategorijaUslugaService : BaseService<KategorijaUslugaDTO, KategorijaUsluga, object>, IKategorijaUslugaService
{
    public KategorijaUslugaService(NuaSpaContext context, IMapper mapper) : base(context, mapper)
    {
    }
}