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

    /// <summary>
    /// References a variable that may have no value. Rendered as <c>$name</c> when the variable holds a value;
    /// otherwise the argument, filter entry or list item holding the reference is omitted and the variable is not declared.
    /// Use it for optional filters; <see cref="Var"/> fails on a missing value instead.
    /// </summary>
    /// <param name="name">The variable name (without the leading <c>$</c>) to reference.</param>
    /// <returns>A reference usable inside an argument object, rendered as <c>$name</c> or omitted.</returns>
    public static GraphQLVariable OptionalVar(string name)
    {
        return new GraphQLVariable(name, optional: true);
    }

    /// <summary>
    /// References a declared variable inside a fluent filter predicate, e.g. <c>x =&gt; x.City == Var&lt;string&gt;("city")</c>.
    /// Rendered as <c>$name</c>. Returns <c>default</c> — the value is never used at runtime; the filter translator reads the call.
    /// </summary>
    /// <typeparam name="T">The CLR type at the comparison site.</typeparam>
    /// <param name="name">The variable name (without the leading <c>$</c>) to reference.</param>
    /// <returns>A default placeholder; the filter emits <c>$name</c>.</returns>
    public static T Var<T>(string name)
    {
        return default;
    }

    /// <summary>
    /// References a variable that may have no value inside a fluent filter predicate, e.g. <c>x =&gt; x.City == OptionalVar&lt;string&gt;("city")</c>.
    /// Rendered as <c>$name</c> when the variable holds a value; otherwise the filter entry is omitted and the variable is not declared.
    /// Returns <c>default</c> — the value is never used at runtime; the filter translator reads the call.
    /// </summary>
    /// <typeparam name="T">The CLR type at the comparison site.</typeparam>
    /// <param name="name">The variable name (without the leading <c>$</c>) to reference.</param>
    /// <returns>A default placeholder; the filter emits <c>$name</c> or nothing.</returns>
    public static T OptionalVar<T>(string name)
    {
        return default;
    }
}
