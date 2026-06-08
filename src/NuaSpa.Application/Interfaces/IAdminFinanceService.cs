using System;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IAdminFinanceService
    {
        Task<AdminFinanceDashboardDto> GetDashboardAsync(
            DateTime from,
            DateTime toExclusive,
            int page,
            int pageSize,
            string? search,
            string? status,
            string? methodCategory,
            int? uslugaId,
            CancellationToken cancellationToken = default);

        Task<AdminFinanceCsvResultDto> GetDashboardCsvAsync(
            DateTime from,
            DateTime toExclusive,
            string? search,
            string? status,
            string? methodCategory,
            int? uslugaId,
            CancellationToken cancellationToken = default);
    }
}
