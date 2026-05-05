using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecenzijaController : ControllerBase
    {
        private readonly IRecenzijaService _service;

        public RecenzijaController(IRecenzijaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecenzijaDTO>>> Get([FromQuery] int uslugaId)
        {
            var result = await _service.GetByUslugaAsync(uslugaId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Klijent,Admin")]
        public async Task<ActionResult<RecenzijaDTO>> Create([FromBody] RecenzijaCreateDTO dto)
        {
            var korisnikId = GetUserId();
            var created = await _service.CreateAsync(korisnikId, dto);
            return Ok(created);
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
}

