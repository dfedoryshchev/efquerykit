namespace EfQueryKit.Parallel;

// run the independent subqueries at the same time, but cap it - was opening too many
// connections at once and draining the pool. each query makes its own DbContext.
public static class FanOut
{
    public static async Task<IReadOnlyList<T>> RunAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> queries, int maxConcurrency, CancellationToken ct = default)
    {
        using var gate = new SemaphoreSlim(maxConcurrency);
        var tasks = queries.Select(async q =>
        {
            await gate.WaitAsync(ct);
            try { return await q(ct); }
            finally { gate.Release(); }
        });
        return await Task.WhenAll(tasks);
    }
}
