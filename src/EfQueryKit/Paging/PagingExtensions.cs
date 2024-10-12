using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EfQueryKit.Paging;

// ok this works. COUNT(*) OVER() puts the total on every row, read it off the first one.
// one round trip now instead of two. caller writes the sql w/ the count col + limit/offset.
public static class PagingExtensions
{
    public static async Task<Page<TRow>> ToPagedResultAsync<TRow>(
        this DatabaseFacade database, FormattableString pagedSql,
        int pageNumber, int pageSize, CancellationToken ct = default)
        where TRow : class, ITotalRow
    {
        var rows = await database.SqlQuery<TRow>(pagedSql).ToListAsync(ct);
        var total = rows.Count == 0 ? 0 : rows[0].TotalCount;
        return new Page<TRow>(rows, total, pageNumber, pageSize);
    }
}
