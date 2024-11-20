namespace EfQueryKit.Parallel;

// run the independent subqueries at the same time instead of one after another.
// each one makes its own DbContext (a single context isnt thread-safe).
public static class FanOut
{
    public static async Task<IReadOnlyList<T>> RunAsync<T>(
        IEnumerable<Func<CancellationToken, Task<T>>> queries, CancellationToken ct = default)
    {
        var tasks = queries.Select(q => q(ct));
        return await Task.WhenAll(tasks);
    }
}
