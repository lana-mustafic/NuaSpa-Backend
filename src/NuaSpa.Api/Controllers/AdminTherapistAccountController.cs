using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/admin/therapists/{zaposlenikId:int}/account")]
[Authorize(Roles = "Admin")]
public class AdminTherapistAccountController : ControllerBase
{
    private readonly ITherapistAccountService _accountService;
    private readonly IConfiguration _configuration;

    public AdminTherapistAccountController(
        ITherapistAccountService accountService,
        IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public async Task<ActionResult<TherapistAccountStatusDto>> GetStatus(int zaposlenikId)
    {
        var status = await _accountService.GetAccountStatusAsync(zaposlenikId);
        if (status == null) return NotFound();
        return Ok(status);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<TherapistInviteResponseDto>> Invite(
        int zaposlenikId,
        [FromBody] TherapistInviteRequestDto? body)
    {
        int? adminId = null;
        if (User.TryGetNuaSpaUserId(out var id))
        {
            adminId = id;
        }

        var baseUrl = _configuration["TherapistInvite:BaseUrl"]?.Trim();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "nuaspa://accept-invite";
        }

        var result = await _accountService.InviteAsync(
            zaposlenikId,
            body?.Email,
            adminId,
            baseUrl);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
