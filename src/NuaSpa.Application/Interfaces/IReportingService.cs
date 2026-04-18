namespace NuaSpa.Application.Interfaces 
{
    public interface IReportingService
    {
        Task<byte[]> GenerateTopUslugeReport();
    }
}