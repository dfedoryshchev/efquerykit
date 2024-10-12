namespace EfQueryKit.Paging;

// row coming back from a paged query - carries the total via COUNT(*) OVER()
public interface ITotalRow
{
    int TotalCount { get; }
}
