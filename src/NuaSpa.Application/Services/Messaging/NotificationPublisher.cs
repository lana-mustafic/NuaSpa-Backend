using Microsoft.Extensions.Configuration;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.Interfaces.Messaging;
using NuaSpa.Application.Messaging;
using NuaSpa.Application.Messaging.Messages;

namespace NuaSpa.Application.Services.Messaging;

public sealed class NotificationPublisher : INotificationPublisher
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly IConfiguration _configuration;

    public NotificationPublisher(IRabbitMqPublisher publisher, IConfiguration configuration)
    {
        _publisher = publisher;
        _configuration = configuration;
    }

    public Task PublishRezervacijaPotvrdaAsync(
        RezervacijaDTO rezervacija,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rezervacija.KorisnikEmail))
        {
            return Task.CompletedTask;
        }

        var message = new RezervacijaPotvrdaMessage
        {
            RezervacijaId = rezervacija.Id,
            ToEmail = rezervacija.KorisnikEmail,
            KorisnikIme = rezervacija.KorisnikIme ?? "Klijent",
            UslugaNaziv = rezervacija.UslugaNaziv ?? "Usluga",
            TerapeutIme = rezervacija.ZaposlenikIme ?? "Terapeut",
            DatumRezervacije = rezervacija.DatumRezervacije,
            Cijena = rezervacija.UslugaCijena,
            IsPotvrdjena = rezervacija.IsPotvrdjena,
        };

        return _publisher.PublishAsync(NuaSpaMessageTypes.RezervacijaPotvrda, message, cancellationToken);
    }

    public Task PublishTherapistInviteAsync(
        TherapistInviteEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        return _publisher.PublishAsync(NuaSpaMessageTypes.TherapistInvite, message, cancellationToken);
    }

    public Task PublishUslugaKreiranaAsync(
        UslugaDTO usluga,
        string? adminEmail,
        CancellationToken cancellationToken = default)
    {
        var message = new UslugaKreiranaMessage
        {
            UslugaId = usluga.Id,
            Naziv = usluga.Naziv,
            KategorijaNaziv = usluga.KategorijaNaziv ?? "—",
            Cijena = usluga.Cijena,
            AdminNotifyEmail = adminEmail ?? _configuration["Email:AdminNotify"],
        };

        return _publisher.PublishAsync(NuaSpaMessageTypes.UslugaKreirana, message, cancellationToken);
    }
}
