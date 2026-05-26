using System;
using NuaSpa.Application.Common;

namespace NuaSpa.Application.SearchObjects
{
    public class RezervacijaSearchObject : IPagedSearch
    {
        public int? KorisnikId { get; set; }

        /// <summary>Samo Admin: filtriraj rezervacije po terapeutu.</summary>
        public int? ZaposlenikId { get; set; }

        public DateTime? Datum { get; set; }
        public bool? IsPotvrdjena { get; set; }
        public bool? IncludeOtkazane { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
    }
}
