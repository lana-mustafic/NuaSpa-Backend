using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IFavoritService
    {
        Task<IEnumerable<UslugaDTO>> GetMyFavoritesAsync(int korisnikId);
        Task<bool> AddAsync(int korisnikId, int uslugaId);
        Task<bool> RemoveAsync(int korisnikId, int uslugaId);
        Task<HashSet<int>> GetMyFavoriteIdsAsync(int korisnikId);
    }
}

