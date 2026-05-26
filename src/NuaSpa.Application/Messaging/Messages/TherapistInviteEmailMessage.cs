namespace NuaSpa.Application.Messaging.Messages;

public sealed class TherapistInviteEmailMessage
{
    public string ToEmail { get; set; } = null!;
    public string TherapistName { get; set; } = null!;
    public string InviteUrl { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
}
