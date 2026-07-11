using System.Runtime.CompilerServices;

namespace FluentGraphQL;

/// <summary>
/// Non-generic base for root query objects, so a builder can hold queries of different types.
/// </summary>
public abstract class GraphQLQueryObject : GraphQLObject
{
}

/// <summary>
/// A root query (or mutation) object selecting fields of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type whose fields are selected.</typeparam>
public class GraphQLQueryObject<T> : GraphQLQueryObject
{
    /// <summary>
    /// Creates a query object for the given GraphQL field name.
    /// </summary>
    /// <param name="name">The GraphQL field name this query targets.</param>
    public GraphQLQueryObject(string name)
    {
        Name = GraphQLName.Validate(name);
    }

    /// <summary>
    /// Gives this query an alias (rendered as <c>alias: name</c>).
    /// </summary>
    /// <param name="aliasName">The alias; ignored when null or whitespace.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> As(string aliasName)
    {
        if (string.IsNullOrWhiteSpace(aliasName) == false)
        {
            AliasName = GraphQLName.Validate(aliasName);
        }

        return this;
    }

    /// <summary>
    /// Sets the query arguments from an anonymous object. Use <see cref="GraphQL.Var"/> for variable references.
    /// </summary>
    /// <typeparam name="TArguments">Type of the arguments object.</typeparam>
    /// <param name="arguments">The arguments as an anonymous object, e.g. <c>new { where = new { id = new { eq = 1 } } }</c>.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> WithArguments<TArguments>(TArguments arguments) where TArguments : class
    {
        Arguments = arguments;

        return this;
    }

    /// <summary>
    /// Removes a previously selected field (e.g. after <see cref="AddEveryFields"/>).
    /// </summary>
    /// <typeparam name="TProperty">Type of the field being removed.</typeparam>
    /// <param name="selector">The field to remove, e.g. <c>x => x.Name</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> Except<TProperty>(
        Func<T, TProperty> selector,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null)
    {
        Fields.RemoveByKey(selectorText.ToMemberName());

        return this;
    }

    /// <summary>
    /// Selects a scalar field, e.g. <c>AddField(x =&gt; x.Id)</c>.
    /// </summary>
    /// <typeparam name="TProperty">Type of the selected field.</typeparam>
    /// <param name="selector">The field to select, e.g. <c>x => x.Id</c>.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> AddField<TProperty>(
        Func<T, TProperty> selector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null)
    {
        Fields.Upsert(new GraphQLQueryObjectField(selectorText.ToMemberName(), aliasName));

        return this;
    }

    /// <summary>
    /// Selects a nested object field and its sub-selection, e.g. <c>AddField(x =&gt; x.Adresse, a =&gt; a.AddField(v =&gt; v.City))</c>.
    /// </summary>
    /// <typeparam name="TProperty">Type of the nested object field.</typeparam>
    /// <param name="selector">The nested field to select, e.g. <c>x => x.Adresse</c>.</param>
    /// <param name="complexPropertySelector">Builds the sub-selection of the nested field.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> AddField<TProperty>(
        Func<T, TProperty> selector,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null) where TProperty : class
    {
        UpsertComplexField(selectorText, complexPropertySelector, aliasName);

        return this;
    }

    /// <summary>
    /// Selects every scalar and enum property of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> AddEveryFields()
    {
        var properties = TypeProperties.Of(typeof(T))
            .Where(property => property.CanWrite && (property.PropertyType.IsPrimitive() || property.PropertyType.IsEnum));

        foreach (var property in properties)
        {
            Fields.Upsert(new GraphQLQueryObjectField(property.Name, null));
        }

        return this;
    }

    /// <summary>
    /// Selects a collection field and its sub-selection, e.g. <c>AddCollectionField(x =&gt; x.Contacts, c =&gt; c.AddEveryFields())</c>.
    /// </summary>
    /// <typeparam name="TProperty">Element type of the selected collection.</typeparam>
    /// <param name="selector">The collection to select, e.g. <c>x => x.Contacts</c>.</param>
    /// <param name="complexPropertySelector">Builds the sub-selection of each element.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same query object, to continue chaining.</returns>
    public GraphQLQueryObject<T> AddCollectionField<TProperty>(
        Func<T, IEnumerable<TProperty>> selector,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null) where TProperty : class
    {
        UpsertComplexField(selectorText, complexPropertySelector, aliasName);

        return this;
    }

    private void UpsertComplexField<TProperty>(
        string selectorText,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName) where TProperty : class
    {
        Fields.Upsert(complexPropertySelector(new GraphQLQueryObjectField<TProperty>(selectorText.ToMemberName(), aliasName)));
    }
}
