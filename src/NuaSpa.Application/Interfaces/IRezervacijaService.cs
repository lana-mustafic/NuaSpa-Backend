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
            bool includeOtkazane = false);

        Task<IEnumerable<RezervacijaDTO>> GetForZaposlenikAsync(
            int zaposlenikId,
            DateTime? datum,
            bool? isPotvrdjena,
            bool includeOtkazane = false);

        Task<RezervacijaDTO> CreateAsync(int korisnikId, RezervacijaCreateDTO dto);

        Task<bool> UpdatePotvrdjenaAsync(int rezervacijaId, bool isPotvrdjena);

        Task<bool> UpdatePotvrdjenaForZaposlenikAsync(int rezervacijaId, int zaposlenikId, bool isPotvrdjena);

        Task<List<DateTime>> GetAvailableSlotsAsync(int zaposlenikId, DateTime date, int slotMinutes = 60);

        Task<bool> CancelAsync(
            int rezervacijaId,
            int? requireKorisnikId,
            int? requireZaposlenikId,
            string? razlogOtkaza);
    }
}

