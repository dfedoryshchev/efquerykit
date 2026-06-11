namespace EfQueryKit.Schema;

/// <summary>
/// Collapses a type-per-column table (a value spread across typed columns plus a discriminator)
/// into one indexed text column, so it can be searched and indexed once.
/// </summary>
public static class ValueSquash
{
    /// <summary>
    /// Builds the DDL for a stored generated column that selects the right typed column by the
    /// <c>type</c> discriminator, plus an index over it.
    /// </summary>
    public static string BuildColumnSql(string table, string column, IReadOnlyDictionary<string, string> typeToColumn)
    {
        var cases = string.Join(" ", typeToColumn.Select(kv => "WHEN '" + kv.Key + "' THEN `" + kv.Value + "`"));
        return "ALTER TABLE `" + table + "` ADD COLUMN `" + column + "` VARCHAR(4096) "
             + "GENERATED ALWAYS AS (CASE `type` " + cases + " END) STORED, "
             + "ADD INDEX `ix_" + column + "` (`" + column + "`)";
    }
}
