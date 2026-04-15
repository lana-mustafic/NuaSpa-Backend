using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Api.Controllers;


public class UslugaController : BaseController<UslugaDTO, UslugaSearchObject>
{
    public UslugaController(IBaseService<UslugaDTO, UslugaSearchObject> service) : base(service)
    {
    }
}