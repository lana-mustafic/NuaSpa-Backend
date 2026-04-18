using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseController<TModel, TSearch> : ControllerBase where TModel : class where TSearch : class
{
    protected readonly IBaseService<TModel, TSearch> _service;

    public BaseController(IBaseService<TModel, TSearch> service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual async Task<IEnumerable<TModel>> Get([FromQuery] TSearch? search = null)
    {
        return await _service.Get(search);
    }

    [HttpGet("{id}")]
    public virtual async Task<TModel> GetById(int id)
    {
        return await _service.GetById(id);
    }

    [HttpPost]
    public virtual async Task<TModel> Insert([FromBody] TModel dto)
    {
        return await _service.Insert(dto);
    }
}