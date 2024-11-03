using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.FullText;

// gave up on the linq translation, dropping to raw sql. boolean mode only for now.
public static class FullTextExtensions
{
    public static IQueryable<T> WhereFullText<T>(this DbSet<T> set, string table, string column, string term)
        where T : class
    {
        var sql = "SELECT * FROM `" + table + "` WHERE MATCH(`" + column + "`) AGAINST ({0} IN BOOLEAN MODE)";
#pragma warning disable EF1002
        return set.FromSqlRaw(sql, term);
#pragma warning restore EF1002
    }
}
