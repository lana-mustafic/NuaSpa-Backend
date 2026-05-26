using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IFavoritService
    {
        Task<PagedResult<UslugaDTO>> GetMyFavoritesAsync(
            int korisnikId,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize);

        Task<bool> AddAsync(int korisnikId, int uslugaId);
        Task<bool> RemoveAsync(int korisnikId, int uslugaId);
        Task<HashSet<int>> GetMyFavoriteIdsAsync(int korisnikId);
    }
}
