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
    [Authorize(Roles = "Klijent,Admin")]
    public class FavoritController : ControllerBase
    {
        private readonly IFavoritService _service;

        public FavoritController(IFavoritService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UslugaDTO>>> GetMyFavorites()
        {
            var userId = GetUserId();
            var list = await _service.GetMyFavoritesAsync(userId);
            return Ok(list);
        }

        [HttpGet("ids")]
        public async Task<ActionResult<HashSet<int>>> GetMyFavoriteIds()
        {
            var userId = GetUserId();
            var ids = await _service.GetMyFavoriteIdsAsync(userId);
            return Ok(ids);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] FavoritCreateDTO dto)
        {
            var userId = GetUserId();
            await _service.AddAsync(userId, dto.UslugaId);
            return Ok();
        }

        [HttpDelete("{uslugaId}")]
        public async Task<ActionResult> Remove(int uslugaId)
        {
            var userId = GetUserId();
            await _service.RemoveAsync(userId, uslugaId);
            return Ok();
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

