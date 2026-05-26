using NuaSpa.Application.Common;

namespace NuaSpa.Application.SearchObjects
{
    public class UslugaSearchObject : IPagedSearch
    {
        public string? Naziv { get; set; }
        public decimal? MaxCijena { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
    }
}
