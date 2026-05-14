using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
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

        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "Admin")]
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
            var toExclusive = (to ?? DateTime.Today).Date.AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-7);

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
        [Authorize(Roles = "Admin")]
        public async Task<FileContentResult> GetAdminDashboardCsv(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search = null,
            [FromQuery] int? minOcjena = null,
            [FromQuery] int? maxOcjena = null,
            [FromQuery] int? uslugaId = null,
            [FromQuery] int? zaposlenikId = null,
            CancellationToken cancellationToken = default)
        {
            var toExclusive = (to ?? DateTime.Today).Date.AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-7);

            var bytes = await _service.GetAdminDashboardCsvAsync(
                fromDt,
                toExclusive,
                search,
                minOcjena,
                maxOcjena,
                uslugaId,
                zaposlenikId,
                cancellationToken);

            return File(bytes, "text/csv; charset=utf-8", "recenzije.csv");
        }

        [HttpPost]
        [Authorize(Roles = "Klijent,Admin")]
        public async Task<ActionResult<RecenzijaDTO>> Create([FromBody] RecenzijaCreateDTO dto)
        {
            var korisnikId = User.GetNuaSpaUserId();
            var created = await _service.CreateAsync(korisnikId, dto);
            return Ok(created);
        }
    }
}
