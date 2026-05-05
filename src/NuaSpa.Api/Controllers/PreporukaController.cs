using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Klijent,Admin,Zaposlenik")]
public class PreporukaController : ControllerBase
{
    private readonly IPreporukaService _service;

    public PreporukaController(IPreporukaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UslugaDTO>>> Get([FromQuery] int take = 10)
    {
        var userId = User.GetNuaSpaUserId();
        var result = await _service.GetForKorisnikAsync(userId, take);
        return Ok(result);
    }
}
