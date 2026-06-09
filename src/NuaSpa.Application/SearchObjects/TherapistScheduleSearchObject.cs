using System;

namespace NuaSpa.Application.SearchObjects;

public class TherapistScheduleSearchObject
{
    /// <summary>Selected schedule day (UTC date).</summary>
    public DateTime? Day { get; set; }

    /// <summary>Month shown in mini calendar (any date within the month).</summary>
    public DateTime? CalendarMonth { get; set; }
}
