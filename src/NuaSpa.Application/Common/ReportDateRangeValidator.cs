namespace NuaSpa.Application.Common;

public static class ReportDateRangeValidator
{
    public const int MaxSpanDays = 366;

    public static bool TryValidate(DateTime from, DateTime to, out string? error)
    {
        if (from == default)
        {
            error = "Query parameter 'from' is required.";
            return false;
        }

        if (to == default)
        {
            error = "Query parameter 'to' is required.";
            return false;
        }

        var start = from.Date;
        var end = to.Date;
        if (end < start)
        {
            error = "Invalid period (to < from).";
            return false;
        }

        if ((end - start).TotalDays > MaxSpanDays)
        {
            error = $"Maximum report range is {MaxSpanDays} days.";
            return false;
        }

        error = null;
        return true;
    }
}
