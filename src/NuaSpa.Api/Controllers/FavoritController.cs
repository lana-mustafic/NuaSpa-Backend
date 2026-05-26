using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
[Authorize(Roles = RoleConstants.KlijentAdmin)]
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
            var userId = User.GetNuaSpaUserId();
            var list = await _service.GetMyFavoritesAsync(userId);
            return Ok(list);
        }

        [HttpGet("ids")]
        public async Task<ActionResult<HashSet<int>>> GetMyFavoriteIds()
        {
            var userId = User.GetNuaSpaUserId();
            var ids = await _service.GetMyFavoriteIdsAsync(userId);
            return Ok(ids);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] FavoritCreateDTO dto)
        {
            var userId = User.GetNuaSpaUserId();
            await _service.AddAsync(userId, dto.UslugaId);
            return Ok();
        }

        [HttpDelete("{uslugaId}")]
        public async Task<ActionResult> Remove(int uslugaId)
        {
            var userId = User.GetNuaSpaUserId();
            await _service.RemoveAsync(userId, uslugaId);
            return Ok();
        }
    }
}

