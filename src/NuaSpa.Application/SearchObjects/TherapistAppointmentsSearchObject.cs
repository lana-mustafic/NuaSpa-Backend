using System;
using NuaSpa.Application.Common;

namespace NuaSpa.Application.SearchObjects;

public class TherapistAppointmentsSearchObject : IPagedSearch
{
    /// <summary>upcoming | today | completed | cancelled</summary>
    public string Tab { get; set; } = "upcoming";

    /// <summary>Reference calendar day for Today tab and counts (UTC date).</summary>
    public DateTime? Day { get; set; }

    public string? Search { get; set; }

    public int? UslugaId { get; set; }

    /// <summary>all | confirmed | pending | cancelled</summary>
    public string StatusFilter { get; set; } = "all";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}
