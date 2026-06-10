namespace EfQueryKit.Paging;

/// <summary>
/// Row type returned by a paged query. The total is carried on every row via
/// <c>COUNT(*) OVER()</c>; the page reads it once and exposes it through <see cref="TotalCount"/>.
/// </summary>
public interface ITotalRow
{
    /// <summary>Total number of rows matching the query, before paging is applied.</summary>
    int TotalCount { get; }
}
