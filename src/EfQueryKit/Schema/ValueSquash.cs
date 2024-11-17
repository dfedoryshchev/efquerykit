namespace EfQueryKit.Schema;

// custom_field had the value spread across typed cols (string/select/bool) + a type col.
// squash into one indexed text col so we can search/index it once.
public static class ValueSquash
{
    public static string BuildColumnSql(string table, string column, IReadOnlyDictionary<string, string> typeToColumn)
    {
        var cases = string.Join(" ", typeToColumn.Select(kv => "WHEN '" + kv.Key + "' THEN `" + kv.Value + "`"));
        return "ALTER TABLE `" + table + "` ADD COLUMN `" + column + "` VARCHAR(4096) "
             + "GENERATED ALWAYS AS (CASE `type` " + cases + " END) STORED, "
             + "ADD INDEX `ix_" + column + "` (`" + column + "`)";
    }
}
