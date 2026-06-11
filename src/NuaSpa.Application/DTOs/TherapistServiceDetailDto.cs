using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.DTOs;

/// <summary>Therapist-scoped service detail for the My Services workspace.</summary>
public class TherapistServiceDetailDto
{
    public UslugaDTO Service { get; set; } = null!;

    /// <summary>Server-computed eligibility (category + specialization match).</summary>
    public bool IsCertified { get; set; }

    public bool IsAuthorized { get; set; }

    public ZaposlenikStatus EmploymentStatus { get; set; } = ZaposlenikStatus.Active;

    /// <summary>Completed appointments performed by this therapist for this service.</summary>
    public int CompletedBookingsCount { get; set; }
}
