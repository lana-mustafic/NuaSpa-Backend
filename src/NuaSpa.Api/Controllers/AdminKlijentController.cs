using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleConstants.Admin)]
public class AdminKlijentController : ControllerBase
{
    private readonly IAdminKlijentService _service;

    public AdminKlijentController(IAdminKlijentService service)
    {
        _service = service;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminClientStatsDto>> Stats(
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        var stats = await _service.GetStatsAsync(q, ct);
        return Ok(stats);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminClientRowDTO>>> Get(
        [FromQuery] KorisnikSearchObject? search = null,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        var rows = await _service.GetAsync(search, q, ct);
        return Ok(rows);
    }

    [HttpPost]
    public async Task<ActionResult<AdminClientRowDTO>> Create(
        [FromBody] AdminKlijentCreateDto dto,
        CancellationToken ct = default)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminClientRowDTO>> GetById(
        int id,
        CancellationToken ct = default)
    {
        var row = await _service.GetByIdAsync(id, ct);
        return Ok(row);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<AdminClientRowDTO>> Patch(
        int id,
        [FromBody] AdminKlijentUpdateDto dto,
        CancellationToken ct = default)
    {
        var updated = await _service.PatchAsync(id, dto, ct);
        return Ok(updated);
    }
}
