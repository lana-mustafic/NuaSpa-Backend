using Microsoft.IdentityModel.Tokens;
using NuaSpa.Application.Common;
using NuaSpa.Application.Interfaces;
using NuaSpa.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NuaSpa.Application.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _key;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
    }

    public string CreateToken(Korisnik korisnik, IList<string> uloge)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, korisnik.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, korisnik.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, korisnik.Email!),
            new Claim("Ime", korisnik.Ime),
            new Claim("Prezime", korisnik.Prezime)
        };

        // Dodavanje uloga u token
        foreach (var uloga in uloge)
        {
            claims.Add(new Claim(ClaimTypes.Role, uloga));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(_settings.DurationInMinutes),
            SigningCredentials = creds,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}