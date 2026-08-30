using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.Paging;

/// <summary>
/// Single round-trip pagination from a normal LINQ query. The caller composes the query with
/// LINQ and projects <see cref="ITotalRow.TotalCount"/> as a placeholder (0); the matching
/// <see cref="PagingCountInterceptor"/> captures the SQL on its way to the database and swaps
/// that placeholder for COUNT(*) OVER(), so the page and its total come back together.
/// <para>
/// The total riding on the rows is what makes one round trip possible, and it is also the one
/// case where a second is unavoidable: a page past the end returns no rows, so there is
/// nothing to carry the total. Rather than report a total of zero for a result set that is not
/// empty - which no caller can tell apart from a genuinely empty set - such a page pays for a
/// single COUNT. Page 0 coming back empty needs no second query, because an empty first page
/// does mean an empty result.
/// </para>
/// </summary>
public static class PagingExtensions
{
    internal const string PagedTag = "efquerykit:paged-with-count";

    public static async Task<Page<TRow>> ToPagedResultAsync<TRow>(
        this IQueryable<TRow> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        where TRow : class, ITotalRow
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegative(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        cancellationToken.ThrowIfCancellationRequested();

        var rows = await query
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .TagWith(PagedTag)
            .ToListAsync(cancellationToken);

        if (rows.Count > 0)
        {
            return new Page<TRow>(rows, rows[0].TotalCount, pageNumber, pageSize);
        }

        // An empty page carries no row to read the total off. On the first page that genuinely
        // means nothing matched; past the last page it does not, so only that case pays for a
        // separate count rather than reporting a total of zero.
        var total = pageNumber == 0 ? 0 : await query.CountAsync(cancellationToken);
        return new Page<TRow>(rows, total, pageNumber, pageSize);
    }
}
