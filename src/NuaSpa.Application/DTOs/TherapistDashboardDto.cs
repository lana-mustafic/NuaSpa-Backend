namespace NuaSpa.Application.DTOs;

public class TherapistDashboardDto
{
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedThisMonth { get; set; }
    public double ProsjecnaOcjena { get; set; }
    public int ReviewCount { get; set; }
    public decimal RevenueThisMonth { get; set; }
}
