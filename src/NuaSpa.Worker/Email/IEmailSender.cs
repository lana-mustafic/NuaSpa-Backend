namespace NuaSpa.Worker.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string? plainText = null, CancellationToken cancellationToken = default);
}
