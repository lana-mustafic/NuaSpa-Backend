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
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        ITherapistAccountService therapistAccountService,
        IAuthService authService,
        IWebHostEnvironment environment)
    {
        _therapistAccountService = therapistAccountService;
        _authService = authService;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        var res = await _authService.LoginAsync(loginRequest, HttpContext.RequestAborted);
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var res = await _authService.RefreshAsync(dto, HttpContext.RequestAborted);
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
    public async Task<ActionResult<AcceptInviteResponseDto>> AcceptInvite(
        [FromBody] AcceptTherapistInviteDto dto)
    {
        var response = await _authService.AcceptInviteAsync(dto, HttpContext.RequestAborted);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto dto)
    {
        var response = await _authService.RequestPasswordResetAsync(
            dto,
            includeDevResetUrl: _environment.IsDevelopment(),
            HttpContext.RequestAborted);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ResetPasswordResponseDto>> ResetPassword(
        [FromBody] ResetPasswordRequestDto dto)
    {
        var response = await _authService.ResetPasswordAsync(dto, HttpContext.RequestAborted);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AccountProfileDto>> GetMe()
    {
        var userId = User.GetNuaSpaUserId();
        var profile = await _authService.GetMeAsync(userId, HttpContext.RequestAborted);
        return Ok(profile);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto? dto)
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

        await _authService.LogoutAsync(
            jti,
            expiresAtUtc,
            dto?.RefreshToken,
            HttpContext.RequestAborted);
        return Ok(new { message = "Uspješno ste se odjavili." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ChangePasswordResponseDto>> ChangePassword(
        [FromBody] ChangePasswordDto dto)
    {
        var userId = User.GetNuaSpaUserId();
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        DateTime? expiresAtUtc = null;
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (long.TryParse(expClaim, out var unixExp))
        {
            expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(unixExp).UtcDateTime;
        }

        var result = await _authService.ChangePasswordAsync(
            userId,
            dto,
            jti,
            expiresAtUtc,
            HttpContext.RequestAborted);
        return Ok(result);
    }
}
