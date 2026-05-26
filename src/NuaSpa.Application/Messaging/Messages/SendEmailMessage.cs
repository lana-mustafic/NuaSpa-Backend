namespace NuaSpa.Application.Messaging.Messages;

public sealed class SendEmailMessage
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
    public string? PlainTextBody { get; set; }
}
