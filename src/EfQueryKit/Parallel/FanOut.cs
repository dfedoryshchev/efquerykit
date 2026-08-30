namespace EfQueryKit.Parallel;

/// <summary>
/// Runs independent subqueries concurrently with a capped degree of parallelism, so a single
/// request does not drain the connection pool. Each query should create its own
/// <c>DbContext</c>, since a single context is not thread-safe.
/// </summary>
public static class FanOut
{
    /// <summary>Runs <paramref name="queries"/> concurrently, at most <paramref name="maxConcurrency"/> at a time.</summary>
    /// <remarks>
    /// The first failure cancels the queries that have not finished, so a fan-out that is already
    /// going to throw stops queueing work instead of running every query to completion.
    /// </remarks>
    public static async Task<IReadOnlyList<T>> RunAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> queries, int maxConcurrency, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        using var gate = new SemaphoreSlim(maxConcurrency);
        using var failed = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var tasks = queries.Select(async q =>
        {
            await gate.WaitAsync(failed.Token);
            try { return await q(failed.Token); }
            catch { failed.Cancel(); throw; }
            finally { gate.Release(); }
        });
        return await Task.WhenAll(tasks);
    }
}
