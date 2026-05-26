using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Application.Interfaces
{
    public interface IAdminKlijentService
    {
        Task<AdminClientStatsDto> GetStatsAsync(string? q, CancellationToken ct);
        Task<List<AdminClientRowDTO>> GetAsync(
            KorisnikSearchObject? search,
            string? q,
            int take,
            CancellationToken ct);

        Task<AdminClientRowDTO> CreateAsync(AdminKlijentCreateDto dto, CancellationToken ct);
        Task<AdminClientRowDTO> GetByIdAsync(int id, CancellationToken ct);
        Task<AdminClientRowDTO> PatchAsync(int id, AdminKlijentUpdateDto dto, CancellationToken ct);
    }
}

