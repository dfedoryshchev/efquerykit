namespace EfQueryKit.Paging;

/// <summary>A page of results together with the total row count.</summary>
/// <typeparam name="T">The row type.</typeparam>
public sealed record Page<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
