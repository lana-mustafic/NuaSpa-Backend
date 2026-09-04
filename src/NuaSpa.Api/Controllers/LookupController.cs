using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

/// <summary>Referentni podaci (šifarnici) za forme i administratorsko održavanje.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly ILookupService _service;

    public LookupController(ILookupService service)
    {
        _service = service;
    }

    [HttpGet("drzave")]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Klijent)]
    public async Task<ActionResult<PagedResult<DrzavaLookupDto>>> GetDrzave(
        [FromQuery] string? naziv = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var list = await _service.GetDrzaveAsync(naziv, page, pageSize, ct);
        return Ok(list);
    }

    [HttpPost("drzave")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<DrzavaLookupDto>> CreateDrzava(
        [FromBody] DrzavaWriteDto dto,
        CancellationToken ct = default)
    {
        var created = await _service.CreateDrzavaAsync(dto, ct);
        return Ok(created);
    }

    [HttpPut("drzave/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<DrzavaLookupDto>> UpdateDrzava(
        int id,
        [FromBody] DrzavaWriteDto dto,
        CancellationToken ct = default)
    {
        var updated = await _service.UpdateDrzavaAsync(id, dto, ct);
        return Ok(updated);
    }

    [HttpDelete("drzave/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult> DeleteDrzava(int id, CancellationToken ct = default)
    {
        await _service.DeleteDrzavaAsync(id, ct);
        return NoContent();
    }

    [HttpGet("gradovi")]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Klijent)]
    public async Task<ActionResult<PagedResult<GradLookupDto>>> GetGradovi(
        [FromQuery] int? drzavaId = null,
        [FromQuery] string? naziv = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var list = await _service.GetGradoviAsync(drzavaId, naziv, page, pageSize, ct);
        return Ok(list);
    }

    [HttpPost("gradovi")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<GradLookupDto>> CreateGrad(
        [FromBody] GradWriteDto dto,
        CancellationToken ct = default)
    {
        var created = await _service.CreateGradAsync(dto, ct);
        return Ok(created);
    }

    [HttpPut("gradovi/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<GradLookupDto>> UpdateGrad(
        int id,
        [FromBody] GradWriteDto dto,
        CancellationToken ct = default)
    {
        var updated = await _service.UpdateGradAsync(id, dto, ct);
        return Ok(updated);
    }

    [HttpDelete("gradovi/{id:int}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult> DeleteGrad(int id, CancellationToken ct = default)
    {
        await _service.DeleteGradAsync(id, ct);
        return NoContent();
    }
}
