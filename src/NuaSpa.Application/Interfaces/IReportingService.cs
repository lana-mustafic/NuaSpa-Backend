using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces 
{
    public interface IReportingService
    {
        Task<byte[]> GenerateTopUslugeReport();

        Task<AdminKpiDTO> GetAdminKpisAsync(DateTime date);
        Task<List<RevenuePointDTO>> GetRevenueSeriesAsync(DateTime from, DateTime to);
        Task<List<ServicePopularityDTO>> GetServicePopularityAsync(DateTime from, DateTime to, int take);
        Task<List<TopSpenderDTO>> GetTopSpendersAsync(DateTime from, DateTime to, int take);
    }
}