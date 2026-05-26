using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class AdminFinanceController : ControllerBase
    {
        private readonly IAdminFinanceService _service;

        public AdminFinanceController(IAdminFinanceService service)
        {
            _service = service;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminFinanceDashboardDto>> GetDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? methodCategory = null,
            [FromQuery] int? uslugaId = null,
            CancellationToken cancellationToken = default)
        {
            var toExclusive = (to?.Date ?? DateTime.Today).AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-30);

            var dto = await _service.GetDashboardAsync(
                fromDt,
                toExclusive,
                page,
                pageSize,
                search,
                status,
                methodCategory,
                uslugaId,
                cancellationToken);

            return Ok(dto);
        }

        [HttpGet("dashboard/csv")]
        public async Task<FileContentResult> GetDashboardCsv(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? methodCategory = null,
            [FromQuery] int? uslugaId = null,
            CancellationToken cancellationToken = default)
        {
            var toExclusive = (to?.Date ?? DateTime.Today).AddDays(1);
            var fromDt = (from?.Date) ?? toExclusive.AddDays(-30);

            var bytes = await _service.GetDashboardCsvAsync(
                fromDt,
                toExclusive,
                search,
                status,
                methodCategory,
                uslugaId,
                cancellationToken);

            var fname = $"placanja_{fromDt:yyyyMMdd}_{toExclusive.AddDays(-1):yyyyMMdd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fname);
        }
    }
}
