using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.Paging;

// does 2 round trips for now (count + the page). want to get it down to one
// with COUNT(*) OVER() but the ef mapping is annoying, todo
public static class PagingExtensions
{
    public static async Task<Page<TRow>> ToPagedResultAsync<TRow>(
        this IQueryable<TRow> query, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync(ct);
        return new Page<TRow>(items, total, pageNumber, pageSize);
    }
}
