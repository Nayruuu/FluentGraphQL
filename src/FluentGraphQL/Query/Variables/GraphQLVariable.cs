namespace FluentGraphQL;

/// <summary>
/// A reference to a declared GraphQL variable, produced by <see cref="GraphQL.Var"/> or <see cref="GraphQL.OptionalVar"/>.
/// </summary>
public sealed class GraphQLVariable
{
    public string Name { get; }

    /// <summary>
    /// Whether the entry holding this reference is omitted when the variable has no value, instead of failing.
    /// </summary>
    public bool Optional { get; }

    public GraphQLVariable(string name, bool optional = false)
    {
        Name = name;
        Optional = optional;
    }
}
