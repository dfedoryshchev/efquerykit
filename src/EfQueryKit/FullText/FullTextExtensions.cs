using Microsoft.EntityFrameworkCore;

namespace EfQueryKit.FullText;

/// <summary>MySQL full-text search mode.</summary>
public enum FullTextMode
{
    /// <summary><c>IN BOOLEAN MODE</c>.</summary>
    Boolean,

    /// <summary><c>IN NATURAL LANGUAGE MODE</c>.</summary>
    NaturalLanguage
}

/// <summary>
/// Full-text search via MySQL <c>MATCH ... AGAINST</c>. EF Core will not translate it from LINQ,
/// so this builds the query as raw SQL. The term is parameterized; the table and column are
/// treated as trusted identifiers.
/// </summary>
public static class FullTextExtensions
{
    /// <summary>Runs a full-text match over <paramref name="column"/> in <paramref name="table"/>.</summary>
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
