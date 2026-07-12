namespace FluentGraphQL;

internal sealed class FilterObject
{
    public List<KeyValuePair<string, object>> Entries { get; } = new List<KeyValuePair<string, object>>();

    public FilterObject Add(string key, object value)
    {
        Entries.Add(new KeyValuePair<string, object>(key, value));

        return this;
    }
}
