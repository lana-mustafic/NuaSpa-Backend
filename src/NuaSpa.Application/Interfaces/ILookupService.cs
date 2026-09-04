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

        Task<DrzavaLookupDto> CreateDrzavaAsync(DrzavaWriteDto dto, CancellationToken ct);
        Task<DrzavaLookupDto> UpdateDrzavaAsync(int id, DrzavaWriteDto dto, CancellationToken ct);
        Task DeleteDrzavaAsync(int id, CancellationToken ct);

        Task<GradLookupDto> CreateGradAsync(GradWriteDto dto, CancellationToken ct);
        Task<GradLookupDto> UpdateGradAsync(int id, GradWriteDto dto, CancellationToken ct);
        Task DeleteGradAsync(int id, CancellationToken ct);
    }
}
