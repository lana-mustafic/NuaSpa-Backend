using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IRecenzijaService
    {
        Task<IEnumerable<RecenzijaDTO>> GetByUslugaAsync(int uslugaId);

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

        Task<byte[]> GetAdminDashboardCsvAsync(
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
    }
}

