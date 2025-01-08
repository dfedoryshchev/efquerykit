using EfQueryKit.Schema;
using EfQueryKit.Search;
using Xunit;

namespace EfQueryKit.Tests;

public class SuffixSearchTests
{
    [Fact]
    public void Reverse_reverses_string()
    {
        Assert.Equal("cba", SuffixSearch.Reverse("abc"));
    }
}

public class ValueSquashTests
{
    [Fact]
    public void BuildColumnSql_emits_case_and_index()
    {
        var sql = ValueSquash.BuildColumnSql("custom_field", "value_text",
            new Dictionary<string, string> { ["string"] = "string_value", ["bool"] = "bool_value" });

        Assert.Contains("GENERATED ALWAYS AS (CASE `type`", sql);
        Assert.Contains("WHEN 'string' THEN `string_value`", sql);
        Assert.Contains("ADD INDEX `ix_value_text`", sql);
    }
}
