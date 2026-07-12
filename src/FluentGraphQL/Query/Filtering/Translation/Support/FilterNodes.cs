namespace FluentGraphQL;

internal static class FilterNodes
{
    public static FilterObject Nest(string[] path, string op, object value)
    {
        var current = new FilterObject().Add(op, value);

        for (var i = path.Length - 1; i >= 0; i--)
        {
            current = new FilterObject().Add(path[i], current);
        }

        return current;
    }

    public static FilterObject Merge(FilterObject left, FilterObject right)
    {
        var leftKeys = new HashSet<string>(left.Entries.Select(entry => entry.Key));

        if (right.Entries.Any(entry => leftKeys.Contains(entry.Key)))
        {
            return new FilterObject().Add("and", new FilterArray { Items = { left, right } });
        }

        var merged = new FilterObject();
        merged.Entries.AddRange(left.Entries);
        merged.Entries.AddRange(right.Entries);

        return merged;
    }
}
