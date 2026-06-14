// Runnable examples for the EfQueryKit utilities. SQLite in-memory so it runs without a
// MySQL server; the MySQL-specific helpers (index hints, full-text) are shown by the SQL
// they produce, since they need a real MySQL to execute.
using EfQueryKit.Hints;
using EfQueryKit.Paging;
using EfQueryKit.Parallel;
using EfQueryKit.Schema;
using EfQueryKit.Search;
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

// 1. paging: compose with LINQ and project TotalCount as a placeholder; the interceptor
//    swaps it for COUNT(*) OVER(), so the page and the total come back in one round trip.
var page = await db.Widgets
    .OrderBy(w => w.Id)
    .Select(w => new WidgetRow { Id = w.Id, Name = w.Name, TotalCount = 0 })
    .ToPagedResultAsync(0, 10);
Console.WriteLine($"paging:    {page.Items.Count} of {page.TotalCount} on page 1");

// 2. index hint: EF will not emit FORCE INDEX, so it is injected into the SQL.
var hinted = IndexHint.Inject(
    "SELECT `w`.`Id` FROM `Widget` AS `w` WHERE `w`.`Name` = 'w1'",
    "Widget", "ix_widget_name", IndexHintKind.Force);
Console.WriteLine($"hint:      {hinted}");

// 3. value squash: one indexed column built from the typed custom-field columns.
var ddl = ValueSquash.BuildColumnSql("custom_field", "value_text",
    new Dictionary<string, string> { ["string"] = "string_value", ["bool"] = "bool_value" });
Console.WriteLine($"squash:    {ddl}");

// 4. suffix search: ends-with becomes a prefix on the reversed value.
Console.WriteLine($"suffix:    reverse(\"abc-1234\") = {SuffixSearch.Reverse("abc-1234")}");

// 5. parallel fan-out: independent queries at once, capped.
var counts = await FanOut.RunAsync(
    new Func<CancellationToken, Task<int>>[]
    {
        _ => Task.FromResult(10),
        _ => Task.FromResult(20),
        _ => Task.FromResult(30),
    },
    maxConcurrency: 2);
Console.WriteLine($"fan-out:   [{string.Join(", ", counts)}]");

// 6. full-text uses MATCH .. AGAINST, which needs MySQL, so it is not executed here.
Console.WriteLine("full-text: see WhereFullText (needs MySQL)");

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
