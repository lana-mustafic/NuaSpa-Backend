using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(Korisnik korisnik, IList<string> uloge);
}