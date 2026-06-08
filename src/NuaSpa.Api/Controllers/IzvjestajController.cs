using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using System.Net.Mime;
using NuaSpa.Application.Common;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class IzvjestajController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<IzvjestajController> _logger;

        public IzvjestajController(
            IReportingService reportingService,
            ILogger<IzvjestajController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        /// <summary>
        /// Generiše i exportuje PDF izvještaj o Top 5 usluga.
        /// </summary>
        [HttpGet("top-usluge")]
        [Produces(MediaTypeNames.Application.Pdf)]
        public async Task<IActionResult> GetTopUslugeReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (!ReportDateRangeValidator.TryValidate(from, to, out var rangeError))
            {
                return BadRequest(rangeError);
            }

            try
            {
                var pdfBytes = await _reportingService.GenerateTopUslugeReport(from, to);
                return File(pdfBytes, MediaTypeNames.Application.Pdf, "Top5Usluga_Izvjestaj.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri generisanju PDF izvještaja top-usluge.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Greška pri generisanju izvještaja. Pokušajte ponovo ili kontaktirajte administratora.");
            }
        }

        [HttpGet("kpi")]
        [ProducesResponseType(typeof(AdminKpiDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<AdminKpiDTO>> GetKpis([FromQuery] DateTime? date = null)
        {
            var d = (date ?? DateTime.Now).Date;
            var kpis = await _reportingService.GetAdminKpisAsync(d);
            return Ok(kpis);
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(List<RevenuePointDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RevenuePointDTO>>> GetRevenue(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (!ReportDateRangeValidator.TryValidate(from, to, out var rangeError))
                return BadRequest(rangeError);
            var data = await _reportingService.GetRevenueSeriesAsync(from, to);
            return Ok(data);
        }

        [HttpGet("service-popularity")]
        [ProducesResponseType(typeof(List<ServicePopularityDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ServicePopularityDTO>>> GetServicePopularity(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int take = 8)
        {
            if (!ReportDateRangeValidator.TryValidate(from, to, out var rangeError))
                return BadRequest(rangeError);
            var data = await _reportingService.GetServicePopularityAsync(from, to, take);
            return Ok(data);
        }

        [HttpGet("top-spenders")]
        [ProducesResponseType(typeof(List<TopSpenderDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TopSpenderDTO>>> GetTopSpenders(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int take = 10)
        {
            if (!ReportDateRangeValidator.TryValidate(from, to, out var rangeError))
                return BadRequest(rangeError);
            var data = await _reportingService.GetTopSpendersAsync(from, to, take);
            return Ok(data);
        }

        [HttpGet("activity-feed")]
        [ProducesResponseType(typeof(List<ActivityFeedItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ActivityFeedItemDto>>> GetActivityFeed(
            [FromQuery] DateTime? day = null,
            [FromQuery] int take = 16,
            CancellationToken cancellationToken = default)
        {
            var d = (day ?? DateTime.UtcNow).Date;
            var data = await _reportingService.GetActivityFeedAsync(d, take, cancellationToken);
            return Ok(data);
        }
    }
}