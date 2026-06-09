using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class TherapistDashboardDto
{
    public string TherapistIme { get; set; } = "Therapist";

    public int TodayAppointments { get; set; }

    /// <summary>Non-cancelled appointments in the next 7 days after today.</summary>
    public int UpcomingAppointments { get; set; }

    public int CompletedThisMonth { get; set; }

    /// <summary>All-time average rating for this therapist.</summary>
    public double ProsjecnaOcjena { get; set; }

    /// <summary>All-time review count for this therapist.</summary>
    public int ReviewCount { get; set; }

    /// <summary>Completed payments attributed to this therapist in the current month.</summary>
    public decimal RevenueThisMonth { get; set; }

    public List<TherapistDashboardAppointmentRowDto> TodaySchedule { get; set; } = new();

    public List<TherapistDashboardAppointmentRowDto> UpcomingSchedule { get; set; } = new();

    public TherapistReviewRowDto? LatestReview { get; set; }
}
