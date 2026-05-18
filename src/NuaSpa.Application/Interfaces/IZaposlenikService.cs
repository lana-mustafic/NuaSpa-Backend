using System;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IZaposlenikService : IBaseService<ZaposlenikDTO, object>
    {
        Task<TherapistAdminProfileDto?> GetAdminProfileAsync(
            int zaposlenikId,
            int maxReviews = 20,
            DateTime? kpiFrom = null,
            DateTime? kpiTo = null);

        Task<TherapistKpiDTO?> GetKpiAsync(int zaposlenikId, DateTime from, DateTime to);

        Task<ZaposlenikDTO?> UpdateAsync(int id, ZaposlenikDTO dto);

        Task<string?> ValidateSpecijalizacijaAsync(int? kategorijaUslugaId, string specijalizacija);

        Task<bool> UpdateInternaNapomenaAsync(int zaposlenikId, string? napomena);
    }
}

