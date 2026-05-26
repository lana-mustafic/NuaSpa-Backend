using NuaSpa.Application.Common;

namespace NuaSpa.Application.SearchObjects;

public class KategorijaUslugaSearchObject : IPagedSearch
{
    public string? Naziv { get; set; }

    /// <summary>Uključi obrisane zapise (admin).</summary>
    public bool IncludeDeleted { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}
