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

    /// <summary>Contact phone (Identity PhoneNumber).</summary>
    public string? Phone { get; set; }

    /// <summary>Home city id when assigned.</summary>
    public int? GradId { get; set; }

    /// <summary>Home city name when available.</summary>
    public string? CityName { get; set; }

    /// <summary>Account registration date.</summary>
    public DateTime? MemberSince { get; set; }

    /// <summary>Completed non-cancelled visits (clients only).</summary>
    public int? TotalVisits { get; set; }

    /// <summary>Total paid amount in KM (clients only).</summary>
    public decimal? TotalSpent { get; set; }

    /// <summary>Most recent non-cancelled visit (clients only).</summary>
    public DateTime? LastVisit { get; set; }

    /// <summary>VIP flag including activity heuristic (clients only).</summary>
    public bool? IsVip { get; set; }
}
