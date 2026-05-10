using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IReportingService
    {
        Task<byte[]> GenerateTopUslugeReport();

        Task<AdminKpiDTO> GetAdminKpisAsync(DateTime date);

        /// <summary>Procjena prihoda za dan + broj novih klijenta (admin).</summary>
        Task<DesktopHomeOverviewDto> GetDesktopHomeOverviewAsync(
            DateTime day,
            bool isAdmin,
            bool isZaposlenik,
            bool isKlijent,
            int currentUserId,
            int zaposlenikIdIfTherapist);

        Task<List<RevenuePointDTO>> GetRevenueSeriesAsync(DateTime from, DateTime to);
        Task<List<ServicePopularityDTO>> GetServicePopularityAsync(DateTime from, DateTime to, int take);
        Task<List<TopSpenderDTO>> GetTopSpendersAsync(DateTime from, DateTime to, int take);
    }
}