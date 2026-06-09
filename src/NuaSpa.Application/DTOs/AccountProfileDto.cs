namespace NuaSpa.Application.DTOs;

/// <summary>Signed-in account snapshot for Settings / profile UI.</summary>
public class AccountProfileDto
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    public bool IsActive { get; set; }

    public bool HasPassword { get; set; }

    public int? ZaposlenikId { get; set; }
}
