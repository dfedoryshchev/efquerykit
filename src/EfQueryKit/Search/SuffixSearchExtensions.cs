using System.Linq.Expressions;

namespace EfQueryKit.Search;

// ends-with cant use an index (leading wildcard). store the col reversed + index it,
// then an ends-with becomes a starts-with (prefix) on the reversed column.
public static class SuffixSearch
{
    public static string Reverse(string value)
    {
        var chars = value.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    // reversedColumn = the generated REVERSE(col) column. reverse the term and match as a prefix.
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
