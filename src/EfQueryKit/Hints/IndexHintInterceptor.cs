using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfQueryKit.Hints;

/// <summary>
/// A command interceptor that reads the index-hint tag added by
/// <see cref="IndexHintExtensions.ForceIndex{T}"/> / <see cref="IndexHintExtensions.UseIndex{T}"/>
/// and injects the hint into the SQL. Register with
/// <c>optionsBuilder.AddInterceptors(new IndexHintInterceptor())</c>.
/// </summary>
public sealed class IndexHintInterceptor : DbCommandInterceptor
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
        var text = command.CommandText;
        foreach (var line in text.Split('\n'))
        {
            var tag = line.TrimStart('-', ' ').TrimEnd('\r');
            if (IndexHintExtensions.TryParseTag(tag, out var table, out var index, out var kind))
            {
                command.CommandText = IndexHint.Inject(text, table, index, kind);
                return;
            }
        }
    }
}
