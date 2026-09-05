namespace NuaSpa.Application.DTOs;

/// <summary>Self-service update of the signed-in user's own contact details.</summary>
public class AccountProfileUpdateDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public int? GradId { get; set; }
}
