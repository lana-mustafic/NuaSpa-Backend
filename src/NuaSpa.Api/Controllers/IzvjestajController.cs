using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.Services;
using NuaSpa.Application.Interfaces;

[ApiController]
[Route("[controller]")]
public class IzvjestajController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public IzvjestajController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("top-usluge")]
    public async Task<IActionResult> GetTopUslugeReport()
    {
        var pdfBytes = await _reportingService.GenerateTopUslugeReport();
        return File(pdfBytes, "application/pdf", "TopUsluge.pdf");
    }
}