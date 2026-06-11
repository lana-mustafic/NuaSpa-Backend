namespace NuaSpa.Application.DTOs;

/// <summary>Aggregated therapist workspace profile (single round-trip for profile screen).</summary>
public class TherapistMyProfileDto
{
    public ZaposlenikDTO Profile { get; set; } = null!;

    /// <summary>Email of the linked login account (may differ from staff record email).</summary>
    public string? LoginEmail { get; set; }

    /// <summary>When the linked user account was registered.</summary>
    public DateTime? AccountLinkedAt { get; set; }

    /// <summary>
    /// Services the therapist is eligible to perform (category + specialization match).
    /// Not a separate certification registry.
    /// </summary>
    public int EligibleServicesCount { get; set; }

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    /// <summary>All-time bookings with status Completed (excludes cancelled).</summary>
    public int AllTimeCompletedSessions { get; set; }

    /// <summary>Completed bookings in the current calendar month (matches dashboard).</summary>
    public int CompletedSessionsThisMonth { get; set; }

    public TherapistProfileNextAppointmentDto? NextAppointment { get; set; }
}

public class TherapistProfileNextAppointmentDto
{
    public int Id { get; set; }
    public DateTime DatumRezervacije { get; set; }
    public string? UslugaNaziv { get; set; }
    public string? KorisnikIme { get; set; }
}
