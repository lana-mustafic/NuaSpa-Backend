using NuaSpa.Application.DTOs;
using NuaSpa.Application.Messaging.Messages;

namespace NuaSpa.Application.Interfaces.Messaging;

/// <summary>Visokonivovski publisher za asinhrone notifikacije (RabbitMQ → Worker).</summary>
public interface INotificationPublisher
{
    Task PublishRezervacijaPotvrdaAsync(RezervacijaDTO rezervacija, CancellationToken cancellationToken = default);
    Task PublishRezervacijaOtkazanaAsync(
        RezervacijaDTO rezervacija,
        string razlogOtkaza,
        string otkazaoUloga,
        CancellationToken cancellationToken = default);
    Task PublishTherapistInviteAsync(TherapistInviteEmailMessage message, CancellationToken cancellationToken = default);
    Task PublishUslugaKreiranaAsync(UslugaDTO usluga, string? adminEmail = null, CancellationToken cancellationToken = default);
}
