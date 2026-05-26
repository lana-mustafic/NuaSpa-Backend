using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SistemskaNotifikacijaController : ControllerBase
{
    private readonly ISistemskaNotifikacijaService _service;

    public SistemskaNotifikacijaController(ISistemskaNotifikacijaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SistemskaNotifikacijaDto>>> GetMine(
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var userId = User.GetNuaSpaUserId();
        var items = await _service.GetForUserAsync(userId, take, ct);
        return Ok(items);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<SistemskaNotifikacijaUnreadDto>> GetUnreadCount(
        CancellationToken ct = default)
    {
        var userId = User.GetNuaSpaUserId();
        var count = await _service.GetUnreadCountAsync(userId, ct);
        return Ok(new SistemskaNotifikacijaUnreadDto { BrojNeprocitanih = count });
    }

    [HttpPatch("{id:int}/procitana")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct = default)
    {
        var userId = User.GetNuaSpaUserId();
        var ok = await _service.MarkReadAsync(userId, id, ct);
        if (!ok)
        {
            return NotFound();
        }

        return Ok();
    }

    [HttpPatch("procitaj-sve")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct = default)
    {
        var userId = User.GetNuaSpaUserId();
        await _service.MarkAllReadAsync(userId, ct);
        return Ok();
    }
}
