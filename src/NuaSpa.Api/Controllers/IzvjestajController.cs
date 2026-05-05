using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Interfaces;
using System.Net.Mime;

namespace NuaSpa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> GetTopUslugeReport()
        {
            try
            {
                var pdfBytes = await _reportingService.GenerateTopUslugeReport();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound("Nije moguće generisati izvještaj. Provjerite da li postoje podaci u bazi.");
                }

                // Vraćamo fajl korisniku. 
                // "application/pdf" osigurava da ga browser/desktop prepozna kao PDF.
                return File(pdfBytes, MediaTypeNames.Application.Pdf, "Top5Usluga_Izvjestaj.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri generisanju PDF izvještaja top-usluge.");
                return BadRequest("Greška pri generisanju izvještaja. Pokušajte ponovo ili kontaktirajte administratora.");
            }
        }
    }
}