using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<Korisnik> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITherapistAccountService _therapistAccountService;
    private readonly NuaSpaContext _context;

    public AccountController(
        UserManager<Korisnik> userManager,
        ITokenService tokenService,
        ITherapistAccountService therapistAccountService,
        NuaSpaContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _therapistAccountService = therapistAccountService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByNameAsync(loginRequest.Username)
            ?? await _userManager.FindByEmailAsync(loginRequest.Username);

        if (user == null) return Unauthorized("Neispravno korisničko ime ili lozinka.");

        if (!user.Status)
        {
            return Unauthorized("Account is deactivated. Contact your spa administrator.");
        }

        if (!await _userManager.HasPasswordAsync(user))
        {
            return Unauthorized(
                "Portal access is not activated yet. Open your invitation link to set a password.");
        }

        var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

        if (!result) return Unauthorized("Neispravno korisničko ime ili lozinka.");

        if (await _userManager.IsInRoleAsync(user, "Zaposlenik") &&
            user.ZaposlenikId is > 0)
        {
            var zStatus = await _context.Zaposlenici.AsNoTracking()
                .Where(z => z.Id == user.ZaposlenikId)
                .Select(z => z.Status)
                .FirstOrDefaultAsync();

            if (zStatus != ZaposlenikStatus.Active)
            {
                return Unauthorized(
                    "Your therapist profile is not active. Contact your spa administrator.");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Expiration = DateTime.Now.AddMinutes(60)
        });
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
        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        var (success, message) = await _therapistAccountService.AcceptInviteAsync(
            dto.Token,
            dto.Password);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }
    [HttpPost("register-test")]
    public async Task<IActionResult> Register()
    {
        var user = new Korisnik
        {
            UserName = "lana",
            Email = "lana@test.com",
            Ime = "Lana",
            Prezime = "Test",
            GradId = 1 // DODAJ OVO! (Stavi ID grada koji sigurno postoji u tvojoj Gradovi tabeli)
        };

        var result = await _userManager.CreateAsync(user, "Lana123!"); // Stavio sam jaču lozinku za svaki slučaj

        if (result.Succeeded) return Ok("Korisnik kreiran!");

        return BadRequest(result.Errors);
    }
}