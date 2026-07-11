namespace FluentGraphQL;

/// <summary>
/// A declared GraphQL variable: its name, value, and (optionally) explicit type. A null <see cref="Type"/> means the type is inferred from the value.
/// </summary>
public class GraphQLParameter
{
    public string Name { get; set; }

    public object Value { get; set; }

    public GraphQLParameterType? Type { get; set; }
}
