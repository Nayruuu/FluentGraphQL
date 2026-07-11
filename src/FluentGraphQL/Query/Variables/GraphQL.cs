namespace FluentGraphQL;

/// <summary>
/// Static entry points for the fluent GraphQL API.
/// </summary>
public static class GraphQL
{
    /// <summary>
    /// References a variable previously declared with <c>AddVariable</c>. Rendered as <c>$name</c>.
    /// A string that is not a <see cref="Var"/> is always treated as a literal, escaped value.
    /// </summary>
    /// <param name="name">The variable name (without the leading <c>$</c>) to reference.</param>
    /// <returns>A reference usable inside an argument object, rendered as <c>$name</c>.</returns>
    public static GraphQLVariable Var(string name)
    {
        return new GraphQLVariable(name);
    }
}
