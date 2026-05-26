using NuaSpa.Application.DTOs;
using NuaSpa.Domain.Entities;

namespace NuaSpa.Application.Interfaces;

public interface ITokenService
{
    IssuedTokenDto CreateToken(Korisnik korisnik, IList<string> uloge);
}