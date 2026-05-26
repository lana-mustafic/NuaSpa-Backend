using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController<TModel, TSearch> : ControllerBase
    where TModel : class
    where TSearch : class
{
    protected readonly IBaseService<TModel, TSearch> _service;

    public BaseController(IBaseService<TModel, TSearch> service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TModel>>> Get([FromQuery] TSearch? search = null)
    {
        var list = await _service.Get(search);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TModel>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return Ok(item);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TModel>> Insert([FromBody] TModel dto)
    {
        var created = await _service.Insert(dto);
        return Ok(created);
    }
}
