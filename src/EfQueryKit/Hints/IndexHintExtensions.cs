using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.Hints;

/// <summary>
/// Tags a query so <see cref="IndexHintInterceptor"/> can inject a MySQL index hint into the
/// generated SQL. EF Core has no first-class index-hint API, so the hint travels as a query tag.
/// </summary>
public static class IndexHintExtensions
{
    internal const string TagPrefix = "efquerykit:index-hint:";

    /// <summary>Forces the given index for the query's main table.</summary>
    public static IQueryable<T> ForceIndex<T>(this IQueryable<T> source, string table, string index)
        => Tag(source, table, index, IndexHintKind.Force);

    /// <summary>Restricts the query's main table to the given index.</summary>
    public static IQueryable<T> UseIndex<T>(this IQueryable<T> source, string table, string index)
        => Tag(source, table, index, IndexHintKind.Use);

    private static IQueryable<T> Tag<T>(IQueryable<T> source, string table, string index, IndexHintKind kind)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(table);
        ArgumentException.ThrowIfNullOrEmpty(index);
        return source.TagWith($"{TagPrefix}{(int)kind}:{table}:{index}");
    }

    internal static bool TryParseTag(string tag, out string table, out string index, out IndexHintKind kind)
    {
        table = index = string.Empty;
        kind = IndexHintKind.Force;
        if (!tag.StartsWith(TagPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = tag[TagPrefix.Length..].Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var k))
        {
            return false;
        }

        kind = (IndexHintKind)k;
        table = parts[1];
        index = parts[2];
        return true;
    }
}
