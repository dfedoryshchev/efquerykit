using System.Text.RegularExpressions;

namespace EfQueryKit.Hints;

/// <summary>The kind of MySQL index hint to apply.</summary>
public enum IndexHintKind
{
    /// <summary><c>FORCE INDEX</c>: the optimiser must use the named index if it can.</summary>
    Force,

    /// <summary><c>USE INDEX</c>: the optimiser is restricted to the named index.</summary>
    Use
}

/// <summary>
/// Rewrites generated SQL to add a MySQL index hint. EF Core cannot emit <c>FORCE INDEX</c> /
/// <c>USE INDEX</c> from LINQ, so the hint is injected into the command text after the table.
/// </summary>
public static class IndexHint
{
    /// <summary>
    /// Inserts an index hint immediately after the first <c>FROM `table` [AS alias]</c> in
    /// <paramref name="commandText"/>. Returns the text unchanged if the table is not found.
    /// </summary>
    public static string Inject(string commandText, string table, string index, IndexHintKind kind)
    {
        ArgumentException.ThrowIfNullOrEmpty(commandText);
        ArgumentException.ThrowIfNullOrEmpty(table);
        ArgumentException.ThrowIfNullOrEmpty(index);

        var keyword = kind == IndexHintKind.Force ? "FORCE INDEX" : "USE INDEX";
        var pattern = $"FROM\\s+`{Regex.Escape(table)}`(\\s+(?:AS\\s+)?`?\\w+`?)?";
        var match = Regex.Match(commandText, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return commandText;
        }

        var insertion = $" {keyword} (`{index}`)";
        return commandText.Insert(match.Index + match.Length, insertion);
    }
}
