using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IRezervacijaService
    {
        Task<IEnumerable<RezervacijaDTO>> GetAsync(
            int? korisnikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false,
            int? zaposlenikId = null);

        Task<IEnumerable<RezervacijaDTO>> GetForZaposlenikAsync(
            int zaposlenikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false);

        Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto);

        Task<RezervacijaDTO?> EditAsync(int rezervacijaId, RezervacijaEditDTO dto);

        Task<bool> UpdatePotvrdjenaAsync(int rezervacijaId, bool isPotvrdjena);

        Task<bool> UpdatePotvrdjenaForZaposlenikAsync(int rezervacijaId, int zaposlenikId, bool isPotvrdjena);

        Task<List<DateTime>> GetAvailableSlotsAsync(int zaposlenikId, DateTime date, int slotMinutes = 60);

        Task<bool> CancelAsync(
            int rezervacijaId,
            int? requireKorisnikId,
            int? requireZaposlenikId,
            string? razlogOtkaza);

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

