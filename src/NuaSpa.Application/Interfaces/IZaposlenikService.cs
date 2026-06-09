using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Application.Interfaces
{
    public interface IZaposlenikService : IBaseService<ZaposlenikDTO, ZaposlenikSearchObject>
    {
        Task<TherapistAdminProfileDto?> GetAdminProfileAsync(
            int zaposlenikId,
            int maxReviews = 20,
            DateTime? kpiFrom = null,
            DateTime? kpiTo = null,
            DateTime? weekStart = null);

        Task<TherapistKpiDTO?> GetKpiAsync(int zaposlenikId, DateTime from, DateTime to);

        Task<TherapistAdminRosterDto> GetAdminRosterAsync(
            DateTime? kpiFrom = null,
            DateTime? kpiTo = null,
            DateTime? weekStart = null);

        Task<ZaposlenikDTO?> UpdateAsync(int id, ZaposlenikDTO dto);

        Task<string?> ValidateSpecijalizacijaAsync(int? kategorijaUslugaId, string specijalizacija);

        Task<bool> UpdateInternaNapomenaAsync(int zaposlenikId, string? napomena);

        Task<IEnumerable<ZaposlenikDTO>> GetForServiceAsync(int uslugaId, bool bookableOnly = true);

        /// <summary>Whether the therapist is allowed to perform the given service (category + specialization).</summary>
        Task<bool> IsEligibleForServiceAsync(int zaposlenikId, int uslugaId, bool requireActive = true);

        Task<IEnumerable<ZaposlenikDTO>> GetForCategoryAsync(
            int kategorijaUslugaId,
            bool bookableOnly = true);

        Task<ZaposlenikDTO?> GetMeAsync(int zaposlenikId);

        Task<ZaposlenikDTO?> UpdateMeAsync(int zaposlenikId, TherapistSelfProfileUpdateDto dto);

        Task<TherapistDashboardDto?> GetDashboardAsync(int zaposlenikId, DateTime? day = null);

        Task<TherapistAppointmentsListDto?> GetMyAppointmentsAsync(
            int zaposlenikId,
            string tab,
            DateTime? day,
            string? search,
            string statusFilter,
            int page,
            int pageSize);

        Task<TherapistScheduleDto?> GetMyScheduleAsync(
            int zaposlenikId,
            DateTime? day,
            DateTime? calendarMonth);

        Task<IReadOnlyList<TherapistReviewRowDto>> GetMyReviewsAsync(
            int zaposlenikId,
            int maxReviews = 30);

        Task DeleteAsync(int id);
    }
}

