namespace NuaSpa.Application.Messaging.Messages;

public sealed class PasswordResetEmailMessage
{
    public string ToEmail { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string ResetUrl { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }
}
