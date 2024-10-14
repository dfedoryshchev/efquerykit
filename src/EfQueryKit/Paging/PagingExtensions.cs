using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.Paging;

// off the raw sql strings now. tag the query + skip/take, and let the interceptor swap the
// TotalCount placeholder for COUNT(*) OVER(). the client just writes linq.
public static class PagingExtensions
{
    internal const string PagedTag = "efquerykit:paged-with-count";

    public static async Task<Page<TRow>> ToPagedResultAsync<TRow>(
        this IQueryable<TRow> query, int pageNumber, int pageSize, CancellationToken ct = default)
        where TRow : class, ITotalRow
    {
        var rows = await query.Skip(pageNumber * pageSize).Take(pageSize).TagWith(PagedTag).ToListAsync(ct);
        var total = rows.Count == 0 ? 0 : rows[0].TotalCount;
        return new Page<TRow>(rows, total, pageNumber, pageSize);
    }
}
