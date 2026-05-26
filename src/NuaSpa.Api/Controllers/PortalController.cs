using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortalController : ControllerBase
{
    private readonly IReportingService _reporting;

    public PortalController(IReportingService reporting)
    {
        _reporting = reporting;
    }

    /// <summary>
    /// Agregati za desktop Home: novi klijenti (samo Admin) i procjena prihoda za dan (uvezeno po ulozi).
    /// </summary>
    [HttpGet("desktop-home-overview")]
    [ProducesResponseType(typeof(DesktopHomeOverviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DesktopHomeOverviewDto>> GetDesktopHomeOverview(
        [FromQuery] DateTime? day = null)
    {
        var isAdmin = User.IsInRole("Admin");
        var isZaposlenik = User.IsInRole("Zaposlenik");
        var isKlijent = User.IsInRole("Klijent");

        var d = (day ?? DateTime.UtcNow).Date;
        var userId = User.GetNuaSpaUserId();
        var zaposId = 0;
        if (isZaposlenik)
        {
            if (!User.TryGetNuaSpaZaposlenikId(out zaposId))
            {
                return Forbid();
            }
        }

        var dto = await _reporting.GetDesktopHomeOverviewAsync(
            d,
            isAdmin,
            isZaposlenik,
            isKlijent,
            userId,
            zaposId);

        return Ok(dto);
    }
}
