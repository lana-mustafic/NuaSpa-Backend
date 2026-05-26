using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces;

public interface IObavijestService
{
    Task<PagedResult<ObavijestDto>> GetPublishedAsync(
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default);

    Task<PagedResult<ObavijestDto>> GetAllAdminAsync(
        int page = 1,
        int pageSize = PaginationConstants.DefaultPageSize,
        CancellationToken ct = default);

    Task<ObavijestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ObavijestDto> CreateAsync(ObavijestCreateDto dto, CancellationToken ct = default);
    Task<ObavijestDto?> UpdateAsync(int id, ObavijestUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
