using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.FullText;

// trying to express MATCH(..) AGAINST(..) from linq. cant get ef to translate it. wip, broken.
public static class FullTextExtensions
{
    public static IQueryable<T> WhereFullText<T>(this DbSet<T> set, string column, string term) where T : class
    {
        // ef wont translate a random method. need to register a db function or drop to raw sql.
        return set.Where(x => Match(column, term));
    }
}
