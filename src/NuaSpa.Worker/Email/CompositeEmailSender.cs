using Microsoft.Extensions.Options;

namespace NuaSpa.Worker.Email;

/// <summary>Smtp ako je uključen, inače outbox datoteke.</summary>
public sealed class CompositeEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtp;
    private readonly SmtpEmailSender _smtpSender;
    private readonly FileOutboxEmailSender _fileSender;

    public CompositeEmailSender(
        IOptions<SmtpOptions> smtp,
        SmtpEmailSender smtpSender,
        FileOutboxEmailSender fileSender)
    {
        _smtp = smtp.Value;
        _smtpSender = smtpSender;
        _fileSender = fileSender;
    }

    public Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainText = null,
        CancellationToken cancellationToken = default)
    {
        if (_smtp.Enabled)
        {
            return _smtpSender.SendAsync(to, subject, htmlBody, plainText, cancellationToken);
        }

        return _fileSender.SendAsync(to, subject, htmlBody, plainText, cancellationToken);
    }
}
