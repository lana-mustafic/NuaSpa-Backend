using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IRecenzijaService
    {
        Task<PagedResult<RecenzijaDTO>> GetByUslugaAsync(
            int uslugaId,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize);

        Task<RecenzijaDTO?> GetByIdAsync(int id);

        Task<IReadOnlyList<ReviewableVisitDto>> GetReviewableVisitsAsync(
            int korisnikId,
            int uslugaId,
            CancellationToken cancellationToken = default);

        Task<RecenzijaDTO> CreateAsync(int korisnikId, RecenzijaCreateDTO dto);

        Task<AdminReviewsDashboardDto> GetAdminDashboardAsync(
            DateTime from,
            DateTime toExclusive,
            int page,
            int pageSize,
            string? search,
            int? minOcjena,
            int? maxOcjena,
            int? uslugaId,
            int? zaposlenikId,
            CancellationToken cancellationToken = default);

        Task<(byte[] Content, bool Truncated)> GetAdminDashboardCsvAsync(
            DateTime from,
            DateTime toExclusive,
            string? search,
            int? minOcjena,
            int? maxOcjena,
            int? uslugaId,
            int? zaposlenikId,
            CancellationToken cancellationToken = default);

        Task<bool> SetAdminOdgovorAsync(
            int recenzijaId,
            string? tekst,
            CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteAsync(
            int recenzijaId,
            CancellationToken cancellationToken = default);
    }
}
