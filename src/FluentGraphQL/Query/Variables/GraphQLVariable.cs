namespace FluentGraphQL;

/// <summary>
/// A reference to a declared GraphQL variable, produced by <see cref="GraphQL.Var"/>.
/// </summary>
public sealed class GraphQLVariable
{
    public string Name { get; }

    public GraphQLVariable(string name)
    {
        Name = name;
    }
}
