namespace EfQueryKit.Search;

// idea: ends-with cant use an index (leading wildcard). store the col reversed + index it,
// then ends-with becomes a prefix search. just the reverse helper for now.
public static class SuffixSearch
{
    public static string Reverse(string value)
    {
        var chars = value.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}
