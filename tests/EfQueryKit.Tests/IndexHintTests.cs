using EfQueryKit.Hints;
using Xunit;

namespace EfQueryKit.Tests;

public class IndexHintTests
{
    [Fact]
    public void Inject_adds_force_index_after_from_table()
    {
        const string sql = "SELECT `w`.`Id` FROM `Widget` AS `w` WHERE `w`.`Status` = 1";
        var result = IndexHint.Inject(sql, "Widget", "ix_widget_status", IndexHintKind.Force);
        Assert.Contains("FROM `Widget` AS `w` FORCE INDEX (`ix_widget_status`)", result);
    }

    [Fact]
    public void Inject_adds_use_index_keyword()
    {
        const string sql = "SELECT `w`.`Id` FROM `Widget` AS `w`";
        var result = IndexHint.Inject(sql, "Widget", "ix_widget_status", IndexHintKind.Use);
        Assert.Contains("USE INDEX (`ix_widget_status`)", result);
    }

    [Fact]
    public void Inject_returns_unchanged_when_table_absent()
    {
        const string sql = "SELECT 1";
        Assert.Equal(sql, IndexHint.Inject(sql, "Widget", "ix", IndexHintKind.Force));
    }

    [Fact]
    public void Tag_round_trips_through_parser()
    {
        var tag = $"{IndexHintExtensions.TagPrefix}0:Widget:ix_widget_status";
        Assert.True(IndexHintExtensions.TryParseTag(tag, out var table, out var index, out var kind));
        Assert.Equal("Widget", table);
        Assert.Equal("ix_widget_status", index);
        Assert.Equal(IndexHintKind.Force, kind);
    }
}
