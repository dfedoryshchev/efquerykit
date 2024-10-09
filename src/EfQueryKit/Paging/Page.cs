namespace EfQueryKit.Paging;

/// <summary>A page of results together with the total row count.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
