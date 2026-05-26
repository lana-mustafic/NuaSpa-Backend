using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;

namespace NuaSpa.Application.Interfaces
{
    public interface IResursiService
    {
        Task<SpaCentarDTO?> GetSpaCentarAsync(CancellationToken ct);
        Task<SpaCentarDTO> UpdateSpaCentarAsync(SpaCentarDTO dto, CancellationToken ct);

        Task<List<RadnoVrijemeDTO>> GetRadnoVrijemeAsync(CancellationToken ct);
        Task<List<RadnoVrijemeDTO>> UpdateRadnoVrijemeAsync(
            List<RadnoVrijemeDTO> items,
            CancellationToken ct);

        Task<List<ProstorijaDTO>> GetProstorijeAsync(CancellationToken ct);
        Task<ProstorijaDTO> CreateProstorijaAsync(ProstorijaDTO dto, CancellationToken ct);
        Task UpdateProstorijaAsync(int id, ProstorijaDTO dto, CancellationToken ct);
        Task DeleteProstorijaAsync(int id, CancellationToken ct);

        Task<List<OpremaDTO>> GetOpremaAsync(CancellationToken ct);
        Task<OpremaDTO> CreateOpremaAsync(OpremaDTO dto, CancellationToken ct);
        Task UpdateOpremaAsync(int id, OpremaDTO dto, CancellationToken ct);
        Task DeleteOpremaAsync(int id, CancellationToken ct);

        Task<ResourceAvailabilityDTO> GetAvailabilityAsync(
            DateTime slot,
            int? excludeRezervacijaId,
            CancellationToken ct);
    }
}

