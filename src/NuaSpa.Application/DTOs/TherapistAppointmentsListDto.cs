using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

public class TherapistAppointmentsListDto
{
    public int UpcomingCount { get; set; }
    public int TodayCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    public TherapistAppointmentRowDto? NextAppointment { get; set; }

    public int Ukupno { get; set; }
    public int Stranica { get; set; }
    public int VelicinaStranice { get; set; }
    public List<TherapistAppointmentRowDto> Items { get; set; } = new();
}
