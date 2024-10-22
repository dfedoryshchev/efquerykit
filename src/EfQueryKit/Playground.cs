namespace EfQueryKit;

// scratch pad - ideas i havent done yet / half-done. ignore.
internal static class Playground
{
    // todo: index hints for the multi-join case (force index per table?)
    // todo: fulltext match -> needs a custom translation, MATCH .. AGAINST
    // todo: search by the end of a string (reverse the column + index it)
    // todo: squash the custom_field typed columns into one indexed col
    // todo: run the independent subqueries in parallel, watch the conn pool

    internal const string Notes = "see todos above";

    // first stab at the count thing before it moved into PagingExtensions:
    // var sql = "select *, count(*) over() as total from t limit ? offset ?";
}
