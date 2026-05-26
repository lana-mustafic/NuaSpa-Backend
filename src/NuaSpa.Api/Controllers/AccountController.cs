using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
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

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest loginRequest)
    {
        var res = await _authService.LoginAsync(loginRequest, HttpContext.RequestAborted);
        return Ok(res);
    }

    [HttpGet("validate-invite")]
    public async Task<ActionResult<ValidateInviteTokenDto>> ValidateInvite([FromQuery] string token)
    {
        var dto = await _therapistAccountService.ValidateInviteTokenAsync(token);
        return Ok(dto);
    }

    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptTherapistInviteDto dto)
    {
        var message = await _authService.AcceptInviteAsync(dto, HttpContext.RequestAborted);
        return Ok(new { message });
    }
}