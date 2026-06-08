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
        private static readonly string[] AllowedStatuses = { "all", "paid", "unpaid", "failed", "refunded" };
        private static readonly string[] AllowedMethods = { "all", "card", "cash", "digital" };

        private readonly IAdminFinanceService _service;

        public AdminFinanceController(IAdminFinanceService service)
        {
            _service = service;
        }

        /// <summary>
        /// Admin finance dashboard. Query: status = all|paid|unpaid|failed|refunded; methodCategory = all|card|cash|digital.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(AdminFinanceDashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            if (!TryNormalizeRange(from, to, out var fromDt, out var toExclusive, out var rangeError))
            {
                return BadRequest(rangeError);
            }

            if (!IsAllowed(status, AllowedStatuses, out var statusError))
            {
                return BadRequest(statusError);
            }

            if (!IsAllowed(methodCategory, AllowedMethods, out var methodError))
            {
                return BadRequest(methodError);
            }

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
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboardCsv(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? methodCategory = null,
            [FromQuery] int? uslugaId = null,
            CancellationToken cancellationToken = default)
        {
            if (!TryNormalizeRange(from, to, out var fromDt, out var toExclusive, out var rangeError))
            {
                return BadRequest(rangeError);
            }

            if (!IsAllowed(status, AllowedStatuses, out var statusError))
            {
                return BadRequest(statusError);
            }

            if (!IsAllowed(methodCategory, AllowedMethods, out var methodError))
            {
                return BadRequest(methodError);
            }

            var result = await _service.GetDashboardCsvAsync(
                fromDt,
                toExclusive,
                search,
                status,
                methodCategory,
                uslugaId,
                cancellationToken);

            var fname = $"placanja_{fromDt:yyyyMMdd}_{toExclusive.AddDays(-1):yyyyMMdd}.csv";
            Response.Headers["X-Export-Truncated"] = result.Truncated ? "true" : "false";
            Response.Headers["X-Export-Rows"] = result.ExportedRows.ToString();
            Response.Headers["X-Export-Total"] = result.TotalMatchingRows.ToString();
            return File(result.Bytes, "text/csv; charset=utf-8", fname);
        }

        private static bool TryNormalizeRange(
            DateTime? from,
            DateTime? to,
            out DateTime fromDt,
            out DateTime toExclusive,
            out string error)
        {
            var todayUtc = DateTime.UtcNow.Date;
            toExclusive = (to?.Date ?? todayUtc).AddDays(1);
            fromDt = from?.Date ?? toExclusive.AddDays(-30);

            if (fromDt > toExclusive.AddDays(-1))
            {
                error = "Invalid date range: 'from' must be on or before 'to'.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool IsAllowed(string? value, string[] allowed, out string error)
        {
            var norm = (value ?? "all").Trim().ToLowerInvariant();
            if (Array.IndexOf(allowed, norm) < 0)
            {
                error = $"Invalid value '{value}'. Allowed: {string.Join(", ", allowed)}.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
