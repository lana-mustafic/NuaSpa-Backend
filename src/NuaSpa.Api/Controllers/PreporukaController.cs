using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var userId = GetUserId();
        var result = await _service.GetForKorisnikAsync(userId, take);
        return Ok(result);
    }

    private int GetUserId()
    {
        var idStr = User.FindFirstValue(JwtRegisteredClaimNames.NameId)
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idStr, out var userId))
        {
            throw new UnauthorizedAccessException("Ne mogu pročitati korisnički id iz JWT-a.");
        }

        return userId;
    }
}
