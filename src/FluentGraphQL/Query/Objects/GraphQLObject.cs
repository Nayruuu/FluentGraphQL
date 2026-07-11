namespace FluentGraphQL;

/// <summary>
/// Base for anything that has a name, an optional alias, arguments and a nested field selection.
/// </summary>
public abstract class GraphQLObject
{
    private List<GraphQLQueryObjectField> _fields;

    public string Name { get; protected set; }

    public string AliasName { get; protected set; }

    public object Arguments { get; internal set; }

    public List<GraphQLQueryObjectField> Fields => _fields ??= new List<GraphQLQueryObjectField>();

    public bool HasFields => _fields is { Count: > 0 };

    public bool HasAliasName()
    {
        return string.IsNullOrWhiteSpace(AliasName) == false;
    }

    public string GetPrincipalKey()
    {
        return HasAliasName() ? AliasName : Name;
    }
}
