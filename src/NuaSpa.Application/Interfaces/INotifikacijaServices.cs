using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuaSpa.Application.DTOs;
using NuaSpa.Domain.Entities;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.Interfaces;

public interface INotificationPushService
{
    Task PushUpdatedAsync(int korisnikId, CancellationToken ct = default);
}

public interface ISistemskaNotifikacijaService
{
    Task NotifyUsersAsync(
        IEnumerable<int> korisnikIds,
        SistemskaNotifikacijaTip tip,
        string naslov,
        string tekst,
        int? rezervacijaId = null,
        CancellationToken ct = default);

    Task NotifyRezervacijaKreiranaAsync(Rezervacija rezervacija, CancellationToken ct = default);
    Task NotifyRezervacijaPotvrdenaAsync(Rezervacija rezervacija, CancellationToken ct = default);
    Task NotifyRezervacijaOtkazanaAsync(Rezervacija rezervacija, string razlog, CancellationToken ct = default);
    Task NotifyRezervacijaZavrsenaAsync(Rezervacija rezervacija, CancellationToken ct = default);
    Task NotifyPlacanjeUspjesnoAsync(Rezervacija rezervacija, decimal iznos, CancellationToken ct = default);
    Task NotifyPlacanjeRefundiranoAsync(Rezervacija rezervacija, decimal iznos, CancellationToken ct = default);

    Task<IReadOnlyList<SistemskaNotifikacijaDto>> GetForUserAsync(
        int korisnikId,
        int take,
        CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(int korisnikId, CancellationToken ct = default);

    Task<bool> MarkReadAsync(int korisnikId, int notifikacijaId, CancellationToken ct = default);

    Task MarkAllReadAsync(int korisnikId, CancellationToken ct = default);
}

public interface IObavijestService
{
    Task<IReadOnlyList<ObavijestDto>> GetPublishedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ObavijestDto>> GetAllAdminAsync(CancellationToken ct = default);
    Task<ObavijestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ObavijestDto> CreateAsync(ObavijestCreateDto dto, CancellationToken ct = default);
    Task<ObavijestDto?> UpdateAsync(int id, ObavijestUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
