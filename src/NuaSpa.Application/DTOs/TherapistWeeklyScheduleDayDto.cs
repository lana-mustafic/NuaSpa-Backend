namespace NuaSpa.Application.DTOs;

public class TherapistWeeklyScheduleDayDto
{
    public int DanUSedmici { get; set; }
    public string Label { get; set; } = null!;
    public string HoursText { get; set; } = null!;
    public bool IsWorking { get; set; }
}
