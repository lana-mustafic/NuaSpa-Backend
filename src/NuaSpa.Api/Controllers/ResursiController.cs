using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Common;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleConstants.Admin)]
public class ResursiController : ControllerBase
{
    private readonly IResursiService _service;

    public ResursiController(IResursiService service)
    {
        _service = service;
    }

    [HttpGet("spa-centar")]
    public async Task<ActionResult<SpaCentarDTO>> GetSpaCentar()
    {
        var dto = await _service.GetSpaCentarAsync(HttpContext.RequestAborted);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpPut("spa-centar")]
    public async Task<ActionResult<SpaCentarDTO>> UpdateSpaCentar([FromBody] SpaCentarDTO dto)
    {
        var updated = await _service.UpdateSpaCentarAsync(dto, HttpContext.RequestAborted);
        return Ok(updated);
    }

    [HttpGet("radno-vrijeme")]
    public async Task<ActionResult<List<RadnoVrijemeDTO>>> GetRadnoVrijeme()
    {
        var list = await _service.GetRadnoVrijemeAsync(HttpContext.RequestAborted);
        return Ok(list);
    }

    [HttpPut("radno-vrijeme")]
    public async Task<ActionResult<List<RadnoVrijemeDTO>>> UpdateRadnoVrijeme([FromBody] List<RadnoVrijemeDTO> items)
    {
        var updated = await _service.UpdateRadnoVrijemeAsync(items, HttpContext.RequestAborted);
        return Ok(updated);
    }

    [HttpGet("prostorije")]
    public async Task<ActionResult<List<ProstorijaDTO>>> GetProstorije()
    {
        var list = await _service.GetProstorijeAsync(HttpContext.RequestAborted);
        return Ok(list);
    }

    [HttpPost("prostorije")]
    public async Task<ActionResult<ProstorijaDTO>> CreateProstorija([FromBody] ProstorijaDTO dto)
    {
        var created = await _service.CreateProstorijaAsync(dto, HttpContext.RequestAborted);
        return Ok(created);
    }

    [HttpPut("prostorije/{id}")]
    public async Task<ActionResult> UpdateProstorija(int id, [FromBody] ProstorijaDTO dto)
    {
        await _service.UpdateProstorijaAsync(id, dto, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpDelete("prostorije/{id}")]
    public async Task<ActionResult> DeleteProstorija(int id)
    {
        await _service.DeleteProstorijaAsync(id, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpGet("oprema")]
    public async Task<ActionResult<List<OpremaDTO>>> GetOprema()
    {
        var list = await _service.GetOpremaAsync(HttpContext.RequestAborted);
        return Ok(list);
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ResourceAvailabilityDTO>> GetAvailability(
        [FromQuery] DateTime slot,
        [FromQuery] int? excludeRezervacijaId = null)
    {
        var availability = await _service.GetAvailabilityAsync(
            slot,
            excludeRezervacijaId,
            HttpContext.RequestAborted);

        return Ok(availability);
    }

    [HttpPost("oprema")]
    public async Task<ActionResult<OpremaDTO>> CreateOprema([FromBody] OpremaDTO dto)
    {
        var created = await _service.CreateOpremaAsync(dto, HttpContext.RequestAborted);
        return Ok(created);
    }

    [HttpPut("oprema/{id}")]
    public async Task<ActionResult> UpdateOprema(int id, [FromBody] OpremaDTO dto)
    {
        await _service.UpdateOpremaAsync(id, dto, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpDelete("oprema/{id}")]
    public async Task<ActionResult> DeleteOprema(int id)
    {
        await _service.DeleteOpremaAsync(id, HttpContext.RequestAborted);
        return Ok();
    }
}

