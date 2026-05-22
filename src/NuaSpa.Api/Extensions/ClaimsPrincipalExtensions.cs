using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NuaSpa.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetNuaSpaUserId(this ClaimsPrincipal user)
    {
        var idStr = user.FindFirstValue(JwtRegisteredClaimNames.NameId)
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(idStr, out var userId))
        {
            throw new UnauthorizedAccessException("Ne mogu pročitati korisnički id iz JWT-a.");
        }

        return userId;
    }

    public static int GetNuaSpaZaposlenikId(this ClaimsPrincipal user)
    {
        if (!user.TryGetNuaSpaZaposlenikId(out var zaposlenikId))
        {
            throw new UnauthorizedAccessException("Korisnik nema ZaposlenikId claim u tokenu.");
        }

        return zaposlenikId;
    }

    public static bool TryGetNuaSpaZaposlenikId(this ClaimsPrincipal user, out int zaposlenikId)
    {
        var idStr = user.FindFirstValue("ZaposlenikId");
        return int.TryParse(idStr, out zaposlenikId) && zaposlenikId > 0;
    }

    public static bool TryGetNuaSpaUserId(this ClaimsPrincipal user, out int userId)
    {
        var idStr = user.FindFirstValue(JwtRegisteredClaimNames.NameId)
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out userId) && userId > 0;
    }
}
