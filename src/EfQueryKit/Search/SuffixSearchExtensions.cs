using System.Linq.Expressions;

namespace EfQueryKit.Search;

/// <summary>
/// "Ends with" search served by an index. A trailing wildcard cannot use a normal index, so the
/// column is stored reversed and indexed; an ends-with then becomes a prefix search on the
/// reversed column.
/// </summary>
public static class SuffixSearch
{
    /// <summary>Reverses a string (used to build the reversed-column value or the search prefix).</summary>
    public static string Reverse(string value)
    {
        var chars = value.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Matches rows whose <paramref name="reversedColumn"/> (a stored <c>REVERSE(col)</c> column)
    /// ends with <paramref name="term"/>, as a prefix search on the reversed value.
    /// </summary>
    public static IQueryable<T> WhereSuffix<T>(
        this IQueryable<T> source, Expression<Func<T, string>> reversedColumn, string term)
    {
        var prefix = Reverse(term);
        var body = Expression.Call(
            reversedColumn.Body,
            typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!,
            Expression.Constant(prefix));
        var lambda = Expression.Lambda<Func<T, bool>>(body, reversedColumn.Parameters);
        return source.Where(lambda);
    }
}
