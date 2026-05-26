using NuaSpa.Application.Common;

namespace NuaSpa.Application.SearchObjects;

public class KorisnikSearchObject : IPagedSearch
{
    /// <summary>Opća pretraga (ime, prezime, email, telefon, username).</summary>
    public string? Q { get; set; }

    public string? Ime { get; set; }
    public string? Prezime { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}
