using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IZaposlenikService : IBaseService<ZaposlenikDTO, object>
    {
        Task<TherapistAdminProfileDto?> GetAdminProfileAsync(int zaposlenikId, int maxReviews = 20);

        Task<bool> UpdateInternaNapomenaAsync(int zaposlenikId, string? napomena);
    }
}

