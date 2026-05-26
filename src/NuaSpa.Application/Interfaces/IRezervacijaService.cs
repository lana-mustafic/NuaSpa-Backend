using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.Common;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IRezervacijaService
    {
        Task<RezervacijaDTO?> GetByIdAsync(int rezervacijaId);

        Task<PagedResult<RezervacijaDTO>> GetAsync(
            int? korisnikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false,
            int? zaposlenikId = null,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize);

        Task<PagedResult<RezervacijaDTO>> GetForZaposlenikAsync(
            int zaposlenikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false,
            int page = 1,
            int pageSize = PaginationConstants.DefaultPageSize);

        Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto, bool isAdminBooking = false);

        /// <summary>Postavlja VIP oznaku na rezervaciji (samo admin).</summary>
        Task<bool> SetIsVipAsync(int rezervacijaId, bool isVip);

        Task<RezervacijaDTO?> EditAsync(int rezervacijaId, RezervacijaEditDTO dto);

        Task<bool> UpdatePotvrdjenaAsync(int rezervacijaId, bool isPotvrdjena, int actorUserId);

        Task<bool> UpdatePotvrdjenaForZaposlenikAsync(
            int rezervacijaId,
            int zaposlenikId,
            bool isPotvrdjena,
            int actorUserId);

        Task<bool> CompleteAsync(int rezervacijaId, int actorUserId, bool allowBeforeEnd = false);

        Task<List<DateTime>> GetAvailableSlotsAsync(
            int zaposlenikId,
            DateTime date,
            int? uslugaId = null,
            int slotMinutes = 60);

        Task<RezervacijaCancelResultDto> CancelAsync(
            int rezervacijaId,
            int? requireKorisnikId,
            int? requireZaposlenikId,
            int actorUserId,
            string razlogOtkaza);

        /// <summary>Uklonjeno — koristite otkazivanje umjesto hard delete.</summary>
        [Obsolete("Hard delete nije dozvoljen. Koristite CancelAsync.")]
        Task<(bool Ok, string? Message)> DeleteAdminAsync(int rezervacijaId);

        Task<List<RezervacijaCalendarItemDTO>> GetCalendarAsync(
            DateTime from,
            DateTime to,
            int? zaposlenikId,
            bool includeOtkazane = false,
            int? uslugaId = null,
            int? prostorijaId = null,
            string? q = null);

        /// <summary>
        /// Povijest termina istog klijenta. Terapeut samo ako postoji zajednička rezervacija s tim klijentom.
        /// </summary>
        Task<List<RezervacijaPovijestItemDto>> GetPovijestZaKlijentaAsync(
            bool isAdmin,
            int zaposlenikIdIfTherapist,
            int korisnikKlijentId,
            int? excludeRezervacijaId,
            int take);
    }
}

