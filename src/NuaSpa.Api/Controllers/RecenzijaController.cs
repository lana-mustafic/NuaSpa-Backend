using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.Common;
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
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<RecenzijaDTO>>> Get(
            [FromQuery] int uslugaId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = PaginationConstants.DefaultPageSize)
        {
            if (uslugaId <= 0)
            {
                return BadRequest("Parametar uslugaId je obavezan.");
            }

            var result = await _service.GetByUslugaAsync(uslugaId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RecenzijaDTO>> GetById(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound();
            }

            return Ok(dto);
        }

        [HttpGet("reviewable-visits")]
        [Authorize(Roles = RoleConstants.KlijentAdmin)]
        public async Task<ActionResult<IReadOnlyList<ReviewableVisitDto>>> GetReviewableVisits(
            [FromQuery] int uslugaId,
            CancellationToken cancellationToken = default)
        {
            if (uslugaId <= 0)
            {
                return BadRequest("Parametar uslugaId je obavezan.");
            }

            var userId = User.GetNuaSpaUserId();
            var visits = await _service.GetReviewableVisitsAsync(userId, uslugaId, cancellationToken);
            return Ok(visits);
        }

        [HttpGet("admin-dashboard")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<ActionResult<AdminReviewsDashboardDto>> GetAdminDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? minOcjena = null,
            [FromQuery] int? maxOcjena = null,
            [FromQuery] int? uslugaId = null,
            [FromQuery] int? zaposlenikId = null,
            CancellationToken cancellationToken = default)
        {
            var toExclusive = (to ?? DateTime.UtcNow).Date.AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-7);
            if (from.HasValue && to.HasValue && to.Value.Date < from.Value.Date)
            {
                return BadRequest("Invalid period (to < from).");
            }

            var dto = await _service.GetAdminDashboardAsync(
                fromDt,
                toExclusive,
                page,
                pageSize,
                search,
                minOcjena,
                maxOcjena,
                uslugaId,
                zaposlenikId,
                cancellationToken);

            return Ok(dto);
        }

        [HttpGet("admin-dashboard/csv")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> GetAdminDashboardCsv(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search = null,
            [FromQuery] int? minOcjena = null,
            [FromQuery] int? maxOcjena = null,
            [FromQuery] int? uslugaId = null,
            [FromQuery] int? zaposlenikId = null,
            CancellationToken cancellationToken = default)
        {
            var toExclusive = (to ?? DateTime.UtcNow).Date.AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-7);
            if (from.HasValue && to.HasValue && to.Value.Date < from.Value.Date)
            {
                return BadRequest("Invalid period (to < from).");
            }

            var (bytes, truncated) = await _service.GetAdminDashboardCsvAsync(
                fromDt,
                toExclusive,
                search,
                minOcjena,
                maxOcjena,
                uslugaId,
                zaposlenikId,
                cancellationToken);

            Response.Headers["X-Export-Truncated"] = truncated ? "true" : "false";
            return File(bytes, "text/csv; charset=utf-8", "recenzije.csv");
        }

        [HttpPatch("{id:int}/admin-odgovor")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> PatchAdminOdgovor(
            int id,
            [FromBody] RecenzijaAdminOdgovorPatchDto? body,
            CancellationToken cancellationToken)
        {
            var ok = await _service.SetAdminOdgovorAsync(id, body?.Tekst, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
        {
            var ok = await _service.SoftDeleteAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.KlijentAdmin)]
        public async Task<ActionResult<RecenzijaDTO>> Create([FromBody] RecenzijaCreateDTO dto)
        {
            var korisnikId = User.GetNuaSpaUserId();
            var created = await _service.CreateAsync(korisnikId, dto);
            return Ok(created);
        }
    }
}
