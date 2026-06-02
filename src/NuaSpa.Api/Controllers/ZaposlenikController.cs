using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.SearchObjects;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ZaposlenikController : BaseController<ZaposlenikDTO, ZaposlenikSearchObject>
    {
        private readonly IZaposlenikService _zaposlenikService;

    public ZaposlenikController(IZaposlenikService service) : base(service)
        {
            _zaposlenikService = service;
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Admin)]
        public override async Task<ActionResult<PagedResult<ZaposlenikDTO>>> Get(
            [FromQuery] ZaposlenikSearchObject? search = null)
        {
            var page = await _zaposlenikService.Get(search);
            return Ok(page);
        }

        [HttpGet("admin-roster")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<TherapistAdminRosterDto>> GetAdminRoster(
            [FromQuery] DateTime? kpiFrom = null,
            [FromQuery] DateTime? kpiTo = null,
            [FromQuery] DateTime? weekStart = null)
        {
            var dto = await _zaposlenikService.GetAdminRosterAsync(kpiFrom, kpiTo, weekStart);
            return Ok(dto);
        }

        /// <summary>Override base route so literal paths like <c>me</c> are not parsed as numeric ids.</summary>
        [HttpGet("{id:int}")]
        public new async Task<ActionResult<ZaposlenikDTO>> GetById(int id)
        {
            var dto = await _zaposlenikService.GetById(id);
            if (dto == null || dto.Id == 0)
            {
                return NotFound();
            }

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.Admin)]
        public override async Task<ActionResult<ZaposlenikDTO>> Insert([FromBody] ZaposlenikDTO dto)
        {
            var created = await _zaposlenikService.Insert(dto);
            return Ok(created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<ZaposlenikDTO>> Update(int id, [FromBody] ZaposlenikDTO dto)
        {
            if (id != dto.Id && dto.Id != 0)
            {
                return BadRequest("ID u ruti i tijelu zahtjeva se ne poklapaju.");
            }

            var updated = await _zaposlenikService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpGet("{id}/admin-profile")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<TherapistAdminProfileDto>> GetAdminProfile(
            int id,
            [FromQuery] int maxReviews = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var dto = await _zaposlenikService.GetAdminProfileAsync(id, maxReviews, from, to);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPatch("{id}/interna-napomena")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> UpdateInternaNapomena(
            int id,
            [FromBody] TherapistNotepadUpdateDto body)
        {
            var ok = await _zaposlenikService.UpdateInternaNapomenaAsync(id, body?.Napomena);
            if (!ok)
            {
                return BadRequest(
                    "Terapeut nema povezan korisnički nalog — interna napomena se ne može spremiti.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _zaposlenikService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("for-service/{uslugaId:int}")]
        [Authorize(Roles = RoleConstants.AdminKlijent)]
        public async Task<ActionResult<IEnumerable<ZaposlenikDTO>>> GetForService(
            int uslugaId,
            [FromQuery] bool bookableOnly = true)
        {
            var list = await _zaposlenikService.GetForServiceAsync(uslugaId, bookableOnly);
            return Ok(list);
        }

        [HttpGet("for-category/{kategorijaUslugaId:int}")]
        [Authorize(Roles = RoleConstants.AdminKlijent)]
        public async Task<ActionResult<IEnumerable<ZaposlenikDTO>>> GetForCategory(
            int kategorijaUslugaId,
            [FromQuery] bool bookableOnly = true)
        {
            var list = await _zaposlenikService.GetForCategoryAsync(
                kategorijaUslugaId,
                bookableOnly);
            return Ok(list);
        }

        [HttpGet("me")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<ZaposlenikDTO>> GetMe()
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var dto = await _zaposlenikService.GetMeAsync(id);
            if (dto == null || dto.Id == 0) return NotFound();
            return Ok(dto);
        }

        [HttpPatch("me")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<ZaposlenikDTO>> UpdateMe([FromBody] TherapistSelfProfileUpdateDto body)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var updated = await _zaposlenikService.UpdateMeAsync(id, body);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpGet("me/dashboard")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<TherapistDashboardDto>> GetMyDashboard([FromQuery] DateTime? day = null)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var dto = await _zaposlenikService.GetDashboardAsync(id, day);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpGet("me/reviews")]
        [Authorize(Roles = RoleConstants.Zaposlenik)]
        public async Task<ActionResult<IReadOnlyList<TherapistReviewRowDto>>> GetMyReviews(
            [FromQuery] int maxReviews = 30)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out var id))
            {
                return Forbid();
            }
            var list = await _zaposlenikService.GetMyReviewsAsync(id, maxReviews);
            return Ok(list);
        }

        [HttpGet("{id}/kpi")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<TherapistKpiDTO>> GetKpis(
            int id,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (to < from) return BadRequest("Neispravan period (to < from).");
            if ((to.Date - from.Date).TotalDays > 366)
            {
                return BadRequest("Period je prevelik (max 366 dana).");
            }

            var dto = await _zaposlenikService.GetKpiAsync(id, from, to);
            if (dto == null) return NotFound();

            return Ok(dto);
        }
    }
}
