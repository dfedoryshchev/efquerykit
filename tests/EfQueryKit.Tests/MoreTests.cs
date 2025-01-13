using EfQueryKit.Parallel;
using EfQueryKit.Schema;
using EfQueryKit.Search;
using Xunit;

namespace EfQueryKit.Tests;

public class SuffixSearchTests
{
    [Fact]
    public void Reverse_reverses_string()
    {
        Assert.Equal("cba", SuffixSearch.Reverse("abc"));
    }
}

public class ValueSquashTests
{
    [Fact]
    public void BuildColumnSql_emits_case_and_index()
    {
        var sql = ValueSquash.BuildColumnSql("custom_field", "value_text",
            new Dictionary<string, string> { ["string"] = "string_value", ["bool"] = "bool_value" });

        Assert.Contains("GENERATED ALWAYS AS (CASE `type`", sql);
        Assert.Contains("WHEN 'string' THEN `string_value`", sql);
        Assert.Contains("ADD INDEX `ix_value_text`", sql);
    }
}

public class FanOutTests
{
    [Fact]
    public async Task RunAsync_respects_max_concurrency()
    {
        var current = 0;
        var peak = 0;
        var gate = new object();
        var queries = Enumerable.Range(0, 20).Select<int, Func<CancellationToken, Task<int>>>(i => async ct =>
        {
            lock (gate) { current++; peak = Math.Max(peak, current); }
            await Task.Delay(20, ct);
            lock (gate) { current--; }
            return i;
        });

        var results = await FanOut.RunAsync(queries, maxConcurrency: 4);

        Assert.Equal(20, results.Count);
        Assert.True(peak <= 4, $"peak concurrency was {peak}");
    }
}
