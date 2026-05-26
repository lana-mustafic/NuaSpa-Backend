namespace NuaSpa.Application.SearchObjects;

public interface IPagedSearch
{
    int Page { get; set; }
    int PageSize { get; set; }
}
