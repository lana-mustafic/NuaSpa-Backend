using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<Korisnik> _userManager;
    private readonly ITokenService _tokenService;

    public AccountController(UserManager<Korisnik> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest loginRequest)
    {
        // 1. Pronađi korisnika u bazi
        var user = await _userManager.FindByNameAsync(loginRequest.Username);

        if (user == null) return Unauthorized("Neispravno korisničko ime ili lozinka.");

        // 2. Provjeri lozinku
        var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

        if (!result) return Unauthorized("Neispravno korisničko ime ili lozinka.");

        // 3. Dohvati uloge korisnika
        var roles = await _userManager.GetRolesAsync(user);

        // 4. Generiši token koristeći tvoj servis iz Taska 3
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.UserName!,
            Expiration = DateTime.Now.AddMinutes(60) // Uskladiti sa JwtSettings
        });
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