using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface ILookupService
    {
        Task<PagedResult<DrzavaLookupDto>> GetDrzaveAsync(
            string? naziv,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize,
            CancellationToken ct = default);

        Task<PagedResult<GradLookupDto>> GetGradoviAsync(
            int? drzavaId,
            string? naziv,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize,
            CancellationToken ct = default);
    }
}
