namespace NuaSpa.Application.DTOs;

/// <summary>Admin day view for a therapist (bookings + open slots).</summary>
public class TherapistDayAvailabilityDto
{
    public DateTime Date { get; set; }
    public int ZaposlenikId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    /// <summary>Active, OnLeave, Inactive.</summary>
    public string TherapistStatus { get; set; } = "Active";
    public bool IsSpaClosed { get; set; }
    public bool IsTherapistUnavailable { get; set; }
    public string? WorkingHoursLabel { get; set; }
    public int AppointmentCount { get; set; }
    /// <summary>off | light | moderate | heavy</summary>
    public string Load { get; set; } = "off";
    public IReadOnlyList<TherapistDayBookedSlotDto> Bookings { get; set; } =
        Array.Empty<TherapistDayBookedSlotDto>();
    public IReadOnlyList<DateTime> AvailableSlots { get; set; } =
        Array.Empty<DateTime>();
}

public class TherapistDayBookedSlotDto
{
    public int RezervacijaId { get; set; }
    public DateTime Start { get; set; }
    public int DurationMinutes { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
