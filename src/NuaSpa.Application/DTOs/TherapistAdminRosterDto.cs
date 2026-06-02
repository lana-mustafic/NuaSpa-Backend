namespace NuaSpa.Application.DTOs;

/// <summary>Aggregated admin therapist roster (one round-trip).</summary>
public class TherapistAdminRosterDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public DateTime KpiFrom { get; set; }
    public DateTime KpiTo { get; set; }
    public IReadOnlyList<TherapistRosterRowDto> Therapists { get; set; } =
        Array.Empty<TherapistRosterRowDto>();
}

public class TherapistRosterRowDto
{
    public ZaposlenikDTO Terapeut { get; set; } = new();
    public double ProsjecnaOcjena { get; set; }
    public int UkupnoRezervacija { get; set; }
    public int BrojRecenzija { get; set; }
    public string Uloga { get; set; } = "Therapist";
    public IReadOnlyList<TherapistRosterDayDto> WeekDays { get; set; } =
        Array.Empty<TherapistRosterDayDto>();
}

public class TherapistRosterDayDto
{
    public DateTime Date { get; set; }
    public int AppointmentCount { get; set; }
    /// <summary>off | light | moderate | heavy</summary>
    public string Load { get; set; } = "off";
}
