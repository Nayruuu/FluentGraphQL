using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FluentGraphQL;

/// <summary>
/// A selected field within a query, which may itself carry a nested selection.
/// </summary>
public class GraphQLQueryObjectField : GraphQLObject
{
    /// <summary>
    /// Creates a field with the given name and optional alias.
    /// </summary>
    /// <param name="name">The GraphQL field name.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>; ignored when null or whitespace.</param>
    public GraphQLQueryObjectField(string name, string aliasName)
    {
        Name = GraphQLName.Validate(name);

        if (string.IsNullOrWhiteSpace(aliasName) == false)
        {
            AliasName = GraphQLName.Validate(aliasName);
        }
    }
}

/// <summary>
/// Builds the sub-selection of a nested object or collection field of element type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Element type whose sub-fields are selected.</typeparam>
public class GraphQLQueryObjectField<T> : GraphQLQueryObjectField where T : class
{
    /// <summary>
    /// Creates a nested field with the given name and optional alias.
    /// </summary>
    /// <param name="name">The GraphQL field name.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>; ignored when null or whitespace.</param>
    public GraphQLQueryObjectField(string name, string aliasName) : base(name, aliasName)
    {
    }

    /// <summary>
    /// Adds a type-safe filter to the <c>where</c> argument on this nested field.
    /// Called more than once, the predicates are combined with AND. Use <see cref="GraphQL.Var{T}"/> for variable references.
    /// </summary>
    /// <param name="predicate">The filter predicate; read to build the filter, never compiled to run.</param>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> Where(Expression<Func<T, bool>> predicate)
    {
        var filter = FilterTranslator.Translate(predicate.Body);
        WhereFilter = WhereFilter is null ? filter : FilterNodes.Merge(WhereFilter, filter);

        return this;
    }

    /// <summary>
    /// Selects a scalar sub-field.
    /// </summary>
    /// <typeparam name="TProperty">Type of the selected sub-field.</typeparam>
    /// <param name="selector">The sub-field to select, e.g. <c>x => x.Id</c>.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> AddField<TProperty>(
        Func<T, TProperty> selector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null)
    {
        Fields.Upsert(new GraphQLQueryObjectField(selectorText.ToMemberName(), aliasName));

        return this;
    }

    /// <summary>
    /// Selects every scalar and enum property of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> AddEveryFields()
    {
        var properties = TypeProperties.Of(typeof(T))
            .Where(property =>
                property.CanWrite && (property.PropertyType.IsPrimitive() || property.PropertyType.IsEnum));

        foreach (var property in properties)
        {
            Fields.Upsert(new GraphQLQueryObjectField(property.Name, null));
        }

        return this;
    }

    /// <summary>
    /// Selects a nested object sub-field and its sub-selection.
    /// </summary>
    /// <typeparam name="TProperty">Type of the nested object sub-field.</typeparam>
    /// <param name="selector">The nested sub-field to select, e.g. <c>x => x.Adresse</c>.</param>
    /// <param name="complexPropertySelector">Builds the sub-selection of the nested field.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> AddField<TProperty>(
        Func<T, TProperty> selector,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null) where TProperty : class
    {
        UpsertComplexField(selectorText, complexPropertySelector, aliasName);

        return this;
    }

    /// <summary>
    /// Selects a nested collection field and its sub-selection.
    /// </summary>
    /// <typeparam name="TProperty">Element type of the nested collection.</typeparam>
    /// <param name="selector">The collection to select, e.g. <c>x => x.Tasks</c>.</param>
    /// <param name="complexPropertySelector">Builds the sub-selection of each element.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> AddCollectionField<TProperty>(
        Func<T, IEnumerable<TProperty>> selector,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null) where TProperty : class
    {
        UpsertComplexField(selectorText, complexPropertySelector, aliasName);

        return this;
    }

    /// <summary>
    /// Selects a nested collection field carrying its own arguments, plus its sub-selection.
    /// </summary>
    /// <typeparam name="TProperty">Element type of the nested collection.</typeparam>
    /// <typeparam name="TArguments">Type of the arguments object.</typeparam>
    /// <param name="selector">The collection to select, e.g. <c>x => x.Tasks</c>.</param>
    /// <param name="arguments">The field arguments as an anonymous object; use <see cref="GraphQL.Var"/> for variable references.</param>
    /// <param name="complexPropertySelector">Builds the sub-selection of each element.</param>
    /// <param name="aliasName">Optional alias rendered as <c>alias: field</c>.</param>
    /// <param name="selectorText">Compiler-supplied from <paramref name="selector"/>; leave unset.</param>
    /// <returns>The same field, to continue chaining.</returns>
    public GraphQLQueryObjectField<T> AddField<TProperty, TArguments>(
        Func<T, IEnumerable<TProperty>> selector,
        TArguments arguments,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName = null,
        [CallerArgumentExpression(nameof(selector))] string selectorText = null)
        where TProperty : class
        where TArguments : class
    {
        UpsertComplexField(selectorText, complexPropertySelector, aliasName).Arguments = arguments;

        return this;
    }

    private GraphQLQueryObjectField UpsertComplexField<TProperty>(
        string selectorText,
        Func<GraphQLQueryObjectField<TProperty>, GraphQLQueryObjectField> complexPropertySelector,
        string aliasName) where TProperty : class
    {
        var field = complexPropertySelector(new GraphQLQueryObjectField<TProperty>(selectorText.ToMemberName(), aliasName));
        Fields.Upsert(field);

        return field;
    }
}
