using System.Text;
using Microsoft.Extensions.Options;

namespace NuaSpa.Worker.Email;

/// <summary>
/// Development / fallback: sprema e-mail kao HTML datoteke (stvarni izlaz, ne samo log).
/// </summary>
public sealed class FileOutboxEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<FileOutboxEmailSender> _logger;

    public FileOutboxEmailSender(IOptions<EmailOptions> options, ILogger<FileOutboxEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainText = null,
        CancellationToken cancellationToken = default)
    {
        var dir = Path.GetFullPath(_options.OutboxDirectory);
        Directory.CreateDirectory(dir);

        var safeTo = string.Concat(to.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeTo}_{Guid.NewGuid():N}.html";
        var path = Path.Combine(dir, fileName);

        var content = new StringBuilder();
        content.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
        content.AppendLine($"<title>{System.Net.WebUtility.HtmlEncode(subject)}</title></head><body>");
        content.AppendLine($"<p><strong>To:</strong> {System.Net.WebUtility.HtmlEncode(to)}</p>");
        content.AppendLine($"<p><strong>Subject:</strong> {System.Net.WebUtility.HtmlEncode(subject)}</p>");
        content.AppendLine("<hr/>");
        content.AppendLine(htmlBody);
        if (!string.IsNullOrWhiteSpace(plainText))
        {
            content.AppendLine("<hr/><pre>");
            content.AppendLine(System.Net.WebUtility.HtmlEncode(plainText));
            content.AppendLine("</pre>");
        }

        content.AppendLine("</body></html>");

        await File.WriteAllTextAsync(path, content.ToString(), Encoding.UTF8, cancellationToken);
        _logger.LogInformation("E-mail spremljen u outbox: {Path}", path);
    }
}
