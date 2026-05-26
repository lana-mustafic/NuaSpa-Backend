using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Api.Extensions;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ITherapistAccountService _therapistAccountService;
    private readonly IAuthService _authService;

    public AccountController(
        ITherapistAccountService therapistAccountService,
        IAuthService authService)
    {
        _therapistAccountService = therapistAccountService;
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        var res = await _authService.LoginAsync(loginRequest, HttpContext.RequestAborted);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpGet("validate-invite")]
    public async Task<ActionResult<ValidateInviteTokenDto>> ValidateInvite([FromQuery] string token)
    {
        var dto = await _therapistAccountService.ValidateInviteTokenAsync(token);
        return Ok(dto);
    }

    [AllowAnonymous]
    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptTherapistInviteDto dto)
    {
        var message = await _authService.AcceptInviteAsync(dto, HttpContext.RequestAborted);
        return Ok(new { message });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti))
        {
            return BadRequest(new { message = "Token nema JTI claim — prijavite se ponovo." });
        }

        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        DateTime expiresAtUtc;
        if (long.TryParse(expClaim, out var unixExp))
        {
            expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(unixExp).UtcDateTime;
        }
        else
        {
            expiresAtUtc = DateTime.UtcNow.AddHours(1);
        }

        await _authService.LogoutAsync(jti, expiresAtUtc, HttpContext.RequestAborted);
        return Ok(new { message = "Uspješno ste se odjavili." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetNuaSpaUserId();
        await _authService.ChangePasswordAsync(userId, dto, HttpContext.RequestAborted);
        return Ok(new { message = "Lozinka je uspješno promijenjena." });
    }
}
