namespace NuaSpa.Application.DTOs;

/// <summary>Admin dashboard activity stream item.</summary>
public class ActivityFeedItemDto
{
    /// <summary>booking | payment | review | client</summary>
    public string Tip { get; set; } = null!;

    public string Naslov { get; set; } = null!;

    public string? Podnaslov { get; set; }

    public DateTime DatumVrijeme { get; set; }
}
