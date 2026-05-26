namespace NuaSpa.Application.Common;

/// <summary>
/// Centralizovane role string vrijednosti (koriste se u [Authorize] atributima i provjerama u kodu).
/// </summary>
public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Klijent = "Klijent";
    public const string Zaposlenik = "Zaposlenik";

    public const string KlijentAdmin = Klijent + "," + Admin;
    public const string AdminKlijent = Admin + "," + Klijent;
    public const string AdminZaposlenik = Admin + "," + Zaposlenik;
    public const string AdminKlijentZaposlenik = Admin + "," + Klijent + "," + Zaposlenik;
}

