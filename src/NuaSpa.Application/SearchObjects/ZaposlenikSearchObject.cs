using NuaSpa.Application.Common;
using NuaSpa.Domain.Enums;

namespace NuaSpa.Application.SearchObjects;

public class ZaposlenikSearchObject : IPagedSearch
{
    public string? Ime { get; set; }
    public string? Prezime { get; set; }

    /// <summary>Pretraga po imenu, prezimenu ili specijalizaciji.</summary>
    public string? Q { get; set; }

    public int? KategorijaUslugaId { get; set; }
    public ZaposlenikStatus? Status { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}
