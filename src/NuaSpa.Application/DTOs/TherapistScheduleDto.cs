using System;
using System.Collections.Generic;

namespace NuaSpa.Application.DTOs;

/// <summary>Operational daily schedule snapshot for therapist My Schedule.</summary>
public class TherapistScheduleDto
{
    public DateTime Day { get; set; }
    public int CalendarYear { get; set; }
    public int CalendarMonth { get; set; }
    public TherapistScheduleDayOverviewDto Overview { get; set; } = new();
    public List<int> MonthMarkerDays { get; set; } = new();
    public TherapistAppointmentRowDto? NextAppointment { get; set; }
    public List<TherapistAppointmentRowDto> Items { get; set; } = new();
    public TherapistScheduleAvailabilitySummaryDto? Availability { get; set; }
}

public class TherapistScheduleDayOverviewDto
{
    public int Total { get; set; }
    public int Confirmed { get; set; }
    public int Pending { get; set; }
    public double HoursBooked { get; set; }
}

public class TherapistScheduleAvailabilitySummaryDto
{
    public string? WorkingHoursLabel { get; set; }
    public int AvailableSlotCount { get; set; }
    public bool IsSpaClosed { get; set; }
    public bool IsTherapistUnavailable { get; set; }
    public string Load { get; set; } = "off";
    public List<DateTime> AvailableSlots { get; set; } = new();
}
