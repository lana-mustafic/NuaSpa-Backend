using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuaSpa.Application.DTOs;
using NuaSpa.Application.SearchObjects;

namespace NuaSpa.Application.Common;

public static class PaginationHelper
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1
            ? PaginationConstants.DefaultPageSize
            : Math.Min(pageSize, PaginationConstants.MaxPageSize);
        return (page, pageSize);
    }

    public static (int Page, int PageSize) FromSearch(IPagedSearch? search)
    {
        if (search == null)
        {
            return (1, PaginationConstants.DefaultPageSize);
        }

        return Normalize(search.Page, search.PageSize);
    }

    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Ukupno = total,
            Stranica = page,
            VelicinaStranice = pageSize,
            Items = items,
        };
    }
}
