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

    [Fact]
    public async Task ToPagedResult_pages_a_linq_query_with_the_total_in_one_query()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .AddInterceptors(new PagingCountInterceptor())
                .Options;
            await using var db = new TestContext(options);
            await db.Database.ExecuteSqlRawAsync("CREATE TABLE Widget (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
            for (var i = 1; i <= 25; i++)
            {
                var name = $"w{i}";
                await db.Database.ExecuteSqlAsync($"INSERT INTO Widget (Id, Name) VALUES ({i}, {name})");
            }

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
}
