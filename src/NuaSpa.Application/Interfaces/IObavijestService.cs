using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces;

public interface IObavijestService
{
    Task<IReadOnlyList<ObavijestDto>> GetPublishedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ObavijestDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<ObavijestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ObavijestDto> CreateAsync(ObavijestCreateDto dto, CancellationToken ct = default);
    Task<ObavijestDto?> UpdateAsync(int id, ObavijestUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
