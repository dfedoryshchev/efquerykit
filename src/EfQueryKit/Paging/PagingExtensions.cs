using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.Paging;

/// <summary>
/// Single round-trip pagination from a normal LINQ query. The caller composes the query with
/// LINQ and projects <see cref="ITotalRow.TotalCount"/> as a placeholder (0); the matching
/// <see cref="PagingCountInterceptor"/> captures the SQL on its way to the database and swaps
/// that placeholder for COUNT(*) OVER(), so the page and its total come back together.
/// </summary>
public static class PagingExtensions
{
    internal const string PagedTag = "efquerykit:paged-with-count";

    public static async Task<Page<TRow>> ToPagedResultAsync<TRow>(
        this IQueryable<TRow> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        where TRow : class, ITotalRow
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await query
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .TagWith(PagedTag)
            .ToListAsync(cancellationToken);

        var total = rows.Count == 0 ? 0 : rows[0].TotalCount;
        return new Page<TRow>(rows, total, pageNumber, pageSize);
    }
}
