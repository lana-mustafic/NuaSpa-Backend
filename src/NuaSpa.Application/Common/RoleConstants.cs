using NuaSpa.Domain.Common;

namespace NuaSpa.Application.Common;

/// <summary>
/// Centralizovane role string vrijednosti (koriste se u [Authorize] atributima i provjerama u kodu).
/// Moraju odgovarati seed ulogama u <see cref="RoleNames"/>.
/// </summary>
public static class RoleConstants
{
    public const string Admin = RoleNames.Admin;
    public const string Klijent = RoleNames.Klijent;
    public const string Zaposlenik = RoleNames.Zaposlenik;

    public const string KlijentAdmin = Klijent + "," + Admin;
    public const string AdminKlijent = Admin + "," + Klijent;
    public const string AdminZaposlenik = Admin + "," + Zaposlenik;
    public const string AdminKlijentZaposlenik = Admin + "," + Klijent + "," + Zaposlenik;
}

