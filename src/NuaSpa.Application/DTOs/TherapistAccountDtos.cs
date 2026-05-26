namespace NuaSpa.Application.DTOs;

public class TherapistAccountStatusDto
{
    public int ZaposlenikId { get; set; }
    public bool HasLinkedAccount { get; set; }
    public string? LinkedEmail { get; set; }
    public string? LinkedUserName { get; set; }
    public bool AccountActive { get; set; }
    public bool HasPassword { get; set; }
    public bool InvitePending { get; set; }
    public DateTime? InviteExpiresAt { get; set; }
    public bool CanInvite { get; set; }
    public string? Message { get; set; }
}

public class TherapistInviteRequestDto
{
    public string? Email { get; set; }
}

public class TherapistInviteResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    /// <summary>Full activation URL (for dev copy or email template).</summary>
    public string? InviteUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? TherapistName { get; set; }
    public string? RecipientEmail { get; set; }
}

public class AcceptTherapistInviteDto
{
    public string Token { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public class ValidateInviteTokenDto
{
    public bool Valid { get; set; }
    public string? TherapistName { get; set; }
    public string? Email { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}
