using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.FullText;

public enum FullTextMode { Boolean, NaturalLanguage }

// raw sql for now. term is parameterized; table/column are identifiers so they go in as-is.
public static class FullTextExtensions
{
    public static IQueryable<T> WhereFullText<T>(
        this DbSet<T> set, string table, string column, string term,
        FullTextMode mode = FullTextMode.Boolean) where T : class
    {
        var modeSql = mode == FullTextMode.Boolean ? "IN BOOLEAN MODE" : "IN NATURAL LANGUAGE MODE";
        var sql = "SELECT * FROM `" + table + "` WHERE MATCH(`" + column + "`) AGAINST ({0} " + modeSql + ")";
#pragma warning disable EF1002
        return set.FromSqlRaw(sql, term);
#pragma warning restore EF1002
    }
}
