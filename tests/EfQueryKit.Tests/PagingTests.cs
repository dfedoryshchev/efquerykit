using EfQueryKit.Paging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EfQueryKit.Tests;

public class PagingTests
{
    private sealed class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class WidgetRow : ITotalRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int TotalCount { get; set; }
    }

    private sealed class TestContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Widget> Widgets => Set<Widget>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Widget>().ToTable("Widget");
    }

    private static async Task<TestContext> SeededContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite(connection)
            .AddInterceptors(new PagingCountInterceptor())
            .Options;
        var db = new TestContext(options);
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE Widget (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
        for (var i = 1; i <= 25; i++)
        {
            var name = $"w{i}";
            await db.Database.ExecuteSqlAsync($"INSERT INTO Widget (Id, Name) VALUES ({i}, {name})");
        }

        return db;
    }

    [Fact]
    public async Task ToPagedResult_pages_a_linq_query_with_the_total_in_one_query()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            await using var db = await SeededContextAsync(connection);

            var result = await db.Widgets
                .OrderBy(w => w.Id)
                .Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 })
                .ToPagedResultAsync(1, 10);

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(10, result.Items.Count);
            Assert.Equal(11, result.Items[0].Id);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task ToPagedResult_keeps_the_total_on_a_page_past_the_end()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            await using var db = await SeededContextAsync(connection);

            var result = await db.Widgets
                .OrderBy(w => w.Id)
                .Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 })
                .ToPagedResultAsync(9, 10);

            Assert.Empty(result.Items);
            Assert.Equal(25, result.TotalCount);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task ToPagedResult_returns_a_zero_total_when_nothing_matches()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            await using var db = await SeededContextAsync(connection);

            var result = await db.Widgets
                .Where(w => w.Name == "nothing")
                .OrderBy(w => w.Id)
                .Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 })
                .ToPagedResultAsync(0, 10);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    [Fact]
    public async Task ToPagedResult_rejects_a_page_number_or_size_out_of_range()
    {
        var query = Array.Empty<WidgetRow>().AsQueryable();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => query.ToPagedResultAsync(-1, 10));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => query.ToPagedResultAsync(0, 0));
    }

    [Fact]
    public async Task ToPagedResult_throws_before_querying_when_already_cancelled()
    {
        var options = new DbContextOptionsBuilder().UseSqlite("DataSource=:memory:").Options;
        await using var db = new TestContext(options);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var query = db.Widgets.Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => query.ToPagedResultAsync(0, 10, cts.Token));
    }
}
