using System.Collections.Generic;
using System.Threading.Tasks;
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
public class PreporukaController : ControllerBase
{
    private readonly IPreporukaService _service;

    public PreporukaController(IPreporukaService service)
    {
        _service = service;
    }

    /// <summary>Content-based preporuke s objašnjenjem (razlogTekst).</summary>
    [HttpGet]
    [Authorize(Roles = RoleConstants.KlijentAdmin)]
    public async Task<ActionResult<IEnumerable<PreporucenaUslugaDto>>> Get(
        [FromQuery] int take = 10)
    {
        var userId = User.GetNuaSpaUserId();
        var result = await _service.GetPreporukeAsync(userId, take);
        return Ok(result);
    }

    /// <summary>Zapis pretrage ili pregleda usluge (signali za recommender).</summary>
    [HttpPost("aktivnost")]
    [Authorize(Roles = RoleConstants.Klijent)]
    public async Task<IActionResult> LogAktivnost([FromBody] KorisnikAktivnostCreateDto dto)
    {
        var userId = User.GetNuaSpaUserId();
        await _service.LogAktivnostAsync(userId, dto);
        return NoContent();
    }
}
