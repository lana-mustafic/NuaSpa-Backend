using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Common;
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
    public async Task<ActionResult<List<DrzavaLookupDto>>> GetDrzave(
        [FromQuery] string? naziv = null,
        CancellationToken ct = default)
    {
        var list = await _service.GetDrzaveAsync(naziv, ct);
        return Ok(list);
    }

    [HttpGet("gradovi")]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Klijent)]
    public async Task<ActionResult<List<GradLookupDto>>> GetGradovi(
        [FromQuery] int? drzavaId = null,
        [FromQuery] string? naziv = null,
        CancellationToken ct = default)
    {
        var list = await _service.GetGradoviAsync(drzavaId, naziv, ct);
        return Ok(list);
    }
}
