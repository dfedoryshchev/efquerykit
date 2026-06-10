using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfQueryKit.Paging;

/// <summary>
/// Captures the SQL on its way to the database and, when the paging tag is present, swaps the
/// TotalCount placeholder (a constant 0 column) for COUNT(*) OVER(), so a paged LINQ query
/// carries its own total. Register with
/// <c>optionsBuilder.AddInterceptors(new PagingCountInterceptor())</c>.
/// </summary>
public sealed class PagingCountInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Rewrite(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Rewrite(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void Rewrite(DbCommand command)
    {
        if (!command.CommandText.Contains(PagingExtensions.PagedTag, StringComparison.Ordinal))
        {
            return;
        }

        command.CommandText = Regex.Replace(
            command.CommandText,
            @"\b0 AS (""TotalCount""|`TotalCount`|\[TotalCount\])",
            "COUNT(*) OVER() AS $1");
    }
}
