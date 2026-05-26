using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

/// <summary>Referentni podaci (šifarnici) za forme i validaciju.</summary>
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
}
