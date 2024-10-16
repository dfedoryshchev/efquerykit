// quick demo. sqlite so it runs without a mysql box. real thing is mysql.
using EfQueryKit.Paging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder()
    .UseSqlite(connection)
    .AddInterceptors(new PagingCountInterceptor())
    .Options;
using var db = new DemoContext(options);

db.Database.ExecuteSqlRaw("CREATE TABLE Widget (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);");
for (var i = 1; i <= 25; i++)
{
    var name = $"w{i}";
    db.Database.ExecuteSql($"INSERT INTO Widget (Id, Name) VALUES ({i}, {name})");
}

// project TotalCount as 0; the interceptor swaps it for COUNT(*) OVER().
var page = await db.Widgets
    .OrderBy(w => w.Id)
    .Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 })
    .ToPagedResultAsync(0, 10);
Console.WriteLine($"page 1: {page.Items.Count} of {page.TotalCount}");

internal sealed class Widget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

internal sealed class WidgetRow : ITotalRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalCount { get; set; }
}

internal sealed class DemoContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Widget> Widgets => Set<Widget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Widget>().ToTable("Widget");
}
