namespace EfQueryKit.Parallel;

/// <summary>
/// Runs independent subqueries concurrently with a capped degree of parallelism, so a single
/// request does not drain the connection pool. Each query should create its own
/// <c>DbContext</c>, since a single context is not thread-safe.
/// </summary>
public static class FanOut
{
    /// <summary>Runs <paramref name="queries"/> concurrently, at most <paramref name="maxConcurrency"/> at a time.</summary>
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
