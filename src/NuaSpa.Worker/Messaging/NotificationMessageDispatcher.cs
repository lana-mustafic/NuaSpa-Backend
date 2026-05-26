using System.Text.Json;
using NuaSpa.Application.Messaging;
using NuaSpa.Application.Messaging.Messages;
using NuaSpa.Worker.Email;

namespace NuaSpa.Worker.Messaging;

public sealed class NotificationMessageDispatcher
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationMessageDispatcher> _logger;

    public NotificationMessageDispatcher(IEmailSender emailSender, ILogger<NotificationMessageDispatcher> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task DispatchAsync(NuaSpaMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker obrađuje poruku {Type} ({CorrelationId})", envelope.Type, envelope.CorrelationId);

        switch (envelope.Type)
        {
            case NuaSpaMessageTypes.SendEmail:
                await HandleSendEmailAsync(envelope.PayloadJson, cancellationToken);
                break;
            case NuaSpaMessageTypes.RezervacijaPotvrda:
                await HandleRezervacijaPotvrdaAsync(envelope.PayloadJson, cancellationToken);
                break;
            case NuaSpaMessageTypes.TherapistInvite:
                await HandleTherapistInviteAsync(envelope.PayloadJson, cancellationToken);
                break;
            case NuaSpaMessageTypes.UslugaKreirana:
                await HandleUslugaKreiranaAsync(envelope.PayloadJson, cancellationToken);
                break;
            default:
                _logger.LogWarning("Nepoznat tip poruke: {Type}", envelope.Type);
                break;
        }
    }

    private async Task HandleSendEmailAsync(string json, CancellationToken ct)
    {
        var msg = JsonSerializer.Deserialize<SendEmailMessage>(json)
            ?? throw new InvalidOperationException("Neispravan SendEmailMessage payload.");
        await _emailSender.SendAsync(msg.To, msg.Subject, msg.HtmlBody, msg.PlainTextBody, ct);
    }

    private async Task HandleRezervacijaPotvrdaAsync(string json, CancellationToken ct)
    {
        var m = JsonSerializer.Deserialize<RezervacijaPotvrdaMessage>(json)
            ?? throw new InvalidOperationException("Neispravan RezervacijaPotvrdaMessage payload.");

        var status = m.IsPotvrdjena ? "potvrđena" : "zaprimljena";
        var html = $"""
            <h2>NuaSpa — rezervacija {status}</h2>
            <p>Poštovani/a {System.Net.WebUtility.HtmlEncode(m.KorisnikIme)},</p>
            <p>Vaša rezervacija <strong>#{m.RezervacijaId}</strong> je {status}.</p>
            <ul>
              <li><strong>Usluga:</strong> {System.Net.WebUtility.HtmlEncode(m.UslugaNaziv)}</li>
              <li><strong>Terapeut:</strong> {System.Net.WebUtility.HtmlEncode(m.TerapeutIme)}</li>
              <li><strong>Termin:</strong> {m.DatumRezervacije:dd.MM.yyyy HH:mm}</li>
              <li><strong>Cijena:</strong> {m.Cijena:N2} KM</li>
            </ul>
            <p>Hvala što ste odabrali NuaSpa.</p>
            """;

        var plain = $"Rezervacija #{m.RezervacijaId}: {m.UslugaNaziv} — {m.DatumRezervacije:dd.MM.yyyy HH:mm}";

        await _emailSender.SendAsync(
            m.ToEmail,
            $"NuaSpa — rezervacija {status} (#{m.RezervacijaId})",
            html,
            plain,
            ct);
    }

    private async Task HandleTherapistInviteAsync(string json, CancellationToken ct)
    {
        var m = JsonSerializer.Deserialize<TherapistInviteEmailMessage>(json)
            ?? throw new InvalidOperationException("Neispravan TherapistInviteEmailMessage payload.");

        var html = $"""
            <h2>NuaSpa — pozivnica za terapeutski portal</h2>
            <p>Poštovani/a {System.Net.WebUtility.HtmlEncode(m.TherapistName)},</p>
            <p>Administrator vas poziva da aktivirate NuaSpa terapeutski račun.</p>
            <p><a href="{System.Net.WebUtility.HtmlEncode(m.InviteUrl)}">Aktiviraj račun</a></p>
            <p>Link vrijedi do: {m.ExpiresAtUtc:dd.MM.yyyy HH:mm} UTC.</p>
            <p>Ako link ne radi, kopirajte URL u preglednik aplikacije.</p>
            """;

        await _emailSender.SendAsync(
            m.ToEmail,
            "NuaSpa — aktivacija terapeutskog računa",
            html,
            $"Aktivirajte račun: {m.InviteUrl}",
            ct);
    }

    private async Task HandleUslugaKreiranaAsync(string json, CancellationToken ct)
    {
        var m = JsonSerializer.Deserialize<UslugaKreiranaMessage>(json)
            ?? throw new InvalidOperationException("Neispravan UslugaKreiranaMessage payload.");

        if (string.IsNullOrWhiteSpace(m.AdminNotifyEmail))
        {
            return;
        }

        var html = $"""
            <h2>Nova usluga u katalogu</h2>
            <ul>
              <li><strong>ID:</strong> {m.UslugaId}</li>
              <li><strong>Naziv:</strong> {System.Net.WebUtility.HtmlEncode(m.Naziv)}</li>
              <li><strong>Kategorija:</strong> {System.Net.WebUtility.HtmlEncode(m.KategorijaNaziv)}</li>
              <li><strong>Cijena:</strong> {m.Cijena:N2} KM</li>
            </ul>
            """;

        await _emailSender.SendAsync(
            m.AdminNotifyEmail,
            $"NuaSpa — nova usluga: {m.Naziv}",
            html,
            cancellationToken: ct);
    }
}
