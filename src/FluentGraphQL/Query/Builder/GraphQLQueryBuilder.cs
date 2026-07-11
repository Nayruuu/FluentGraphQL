using System.Text;
using System.Text.Json;
using System.Collections;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;

namespace FluentGraphQL;

/// <summary>
/// Builds a GraphQL query or mutation and its variables from a fluent, strongly-typed description.
/// </summary>
/// <remarks>
/// Configure a builder from a single thread. Once configured, <see cref="Query"/> and <see cref="Variables"/>
/// may be read concurrently: they allocate their own output per call and never mutate shared state.
/// Do not add queries or variables while another thread is reading.
/// </remarks>
public class GraphQLQueryBuilder : GraphQLBuilder
{
    [ThreadStatic]
    private static StringBuilder _cachedBuilder;
    
    private static readonly JsonSerializerOptions ArgumentSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions VariablesSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly bool _mutation;
    private readonly Dictionary<string, GraphQLQueryObject> _queries;

    /// <summary>
    /// The generated GraphQL document. Built on each access.
    /// </summary>
    public string Query => BuildQuery();

    /// <summary>
    /// The declared variables and their values, camelCased, ready to send alongside <see cref="Query"/>.
    /// </summary>
    public JsonObject Variables
    {
        get
        {
            var jsonObject = new JsonObject();

            if (HasParameters)
            {
                foreach (var parameter in Parameters)
                {
                    if (parameter.Value?.Value is not null)
                    {
                        jsonObject[parameter.Key.ToCamelCase()] = JsonSerializer.SerializeToNode(parameter.Value.Value, VariablesSerializerOptions);
                    }
                }
            }

            return jsonObject;
        }
    }

    /// <summary>
    /// The number of root queries (or mutations) currently added.
    /// </summary>
    public int QueriesCount => _queries.Count;

    /// <summary>
    /// The <see cref="Query"/> and <see cref="Variables"/> bundled as a request payload.
    /// </summary>
    public GraphQLRequest Request => new(Query, Variables);

    /// <summary>
    /// Creates a builder for a query, or for a mutation when <paramref name="mutation"/> is true.
    /// </summary>
    /// <param name="mutation">Whether to emit a <c>mutation</c> rather than a <c>query</c>.</param>
    public GraphQLQueryBuilder(bool mutation = false)
    {
        _mutation = mutation;
        _queries = new Dictionary<string, GraphQLQueryObject>();
    }

    /// <summary>
    /// Declares a variable from a pre-built parameter.
    /// </summary>
    /// <param name="parameter">The variable to declare.</param>
    /// <returns>The same builder, to continue chaining.</returns>
    public GraphQLQueryBuilder AddVariable(GraphQLParameter parameter)
    {
        Parameters[GraphQLName.Validate(parameter.Name)] = parameter;

        return this;
    }

    /// <summary>
    /// Declares several variables in one call.
    /// </summary>
    /// <param name="parameters">The variables to declare.</param>
    /// <returns>The same builder, to continue chaining.</returns>
    public GraphQLQueryBuilder AddVariables(params GraphQLParameter[] parameters)
    {
        foreach (var parameter in parameters)
        {
            AddVariable(parameter);
        }

        return this;
    }

    /// <summary>
    /// Declares a variable with an explicit GraphQL type, rather than inferring it from the value.
    /// </summary>
    /// <param name="name">The variable name (without the leading <c>$</c>).</param>
    /// <param name="type">The explicit GraphQL type of the variable.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>The same builder, to continue chaining.</returns>
    public GraphQLQueryBuilder AddVariable(string name, GraphQLParameterType type, object value)
    {
        AddVariable(new GraphQLParameter { Name = name, Type = type, Value = value });

        return this;
    }

    /// <summary>
    /// Declares a variable, inferring its GraphQL type from the value's CLR type.
    /// </summary>
    /// <param name="name">The variable name (without the leading <c>$</c>).</param>
    /// <param name="value">The variable value; its CLR type determines the GraphQL type.</param>
    /// <returns>The same builder, to continue chaining.</returns>
    public GraphQLQueryBuilder AddVariable(string name, object value)
    {
        AddVariable(new GraphQLParameter { Name = name, Value = value });

        return this;
    }

    /// <summary>
    /// Adds a root query (or mutation) object.
    /// </summary>
    /// <typeparam name="T">The type selected by the query object.</typeparam>
    /// <param name="queryObject">The root query to add.</param>
    /// <returns>The same builder, to continue chaining.</returns>
    /// <exception cref="InvalidOperationException">A query with the same name or alias was already added.</exception>
    public GraphQLQueryBuilder AddQuery<T>(GraphQLQueryObject<T> queryObject) where T : class
    {
        var queryName = queryObject.HasAliasName() ? queryObject.AliasName : queryObject.Name;

        if (_queries.ContainsKey(queryName))
        {
            throw new InvalidOperationException($"A query named '{queryName}' has already been added. Use As(...) to give it a distinct alias.");
        }

        _queries[queryName] = queryObject;

        return this;
    }

    private static StringBuilder RentBuilder()
    {
        var builder = _cachedBuilder;

        if (builder is null)
        {
            return new StringBuilder(256);
        }

        _cachedBuilder = null;
        builder.Clear();

        return builder;
    }

    private static string ReturnBuilder(StringBuilder builder)
    {
        var result = builder.ToString();
        _cachedBuilder = builder;

        return result;
    }

    private string BuildQuery()
    {
        var builder = RentBuilder();

        builder.AppendLine(_mutation ? "mutation" : "query");
        AppendParameters(builder);
        builder.AppendLine(" {");

        foreach (var query in _queries.Values)
        {
            AppendQueryObject(builder, query);
        }

        builder.AppendLine("}");

        return ReturnBuilder(builder);
    }

    private void AppendParameters(StringBuilder builder)
    {
        if (HasParameters == false)
        {
            return;
        }

        var declaredParameters = Parameters.Values
            .Where(parameter => parameter.Value is not null)
            .Select(parameter => $"${parameter.Name}: {GraphQLTypeName.Resolve(parameter)}")
            .ToList();

        if (declaredParameters.Any())
        {
            builder.Append(" (").Append(string.Join(", ", declaredParameters)).Append(")");
        }
    }

    private void AppendQueryObject(StringBuilder builder, GraphQLQueryObject query)
    {
        if (query.HasAliasName())
        {
            builder.Append(query.AliasName).Append(": ");
        }

        builder.Append(query.Name);

        if (query.Arguments is not null)
        {
            builder.Append("(");
            AppendArguments(builder, query.Arguments);
            builder.Append(")");
        }

        builder.AppendLine(" {");
        AppendFields(builder, query.Fields);
        builder.AppendLine("}");
    }

    private void AppendArguments(StringBuilder builder, object arguments)
    {
        var properties = TypeProperties.Of(arguments.GetType());
        var first = true;

        foreach (var property in properties)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }

            AppendCamelCase(builder, property.Name);
            builder.Append(": ");
            AppendValue(builder, property.GetValue(arguments));
        }
    }

    private void AppendValue(StringBuilder builder, object value)
    {
        switch (value)
        {
            case null:
                builder.Append("null");
                break;
            case GraphQLVariable reference:
                AppendVariableReference(builder, reference);
                break;
            case string:
                builder.Append(FormatQueryArgument(value));
                break;
            case IDictionary:
                throw new InvalidOperationException("A dictionary cannot be used as an argument value. Use an anonymous object for an input object, or a list for a GraphQL list.");
            case IEnumerable items:
                AppendList(builder, items);
                break;
            case not null when value.GetType().IsClass:
                builder.Append("{ ");
                AppendArguments(builder, value);
                builder.Append(" }");
                break;
            default:
                builder.Append(FormatQueryArgument(value));
                break;
        }
    }

    private void AppendVariableReference(StringBuilder builder, GraphQLVariable reference)
    {
        if (HasParameters == false
            || Parameters.TryGetValue(reference.Name, out var parameter) == false
            || parameter.Value is null)
        {
            throw new InvalidOperationException(
                $"Variable '${reference.Name}' is referenced but was never declared with a value. Declare it with AddVariable(\"{reference.Name}\", ...).");
        }

        builder.Append('$').Append(reference.Name);
    }

    private void AppendList(StringBuilder builder, IEnumerable items)
    {
        builder.Append("[ ");

        var first = true;

        foreach (var item in items)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }

            AppendValue(builder, item);
        }

        builder.Append(" ]");
    }

    private void AppendFields(StringBuilder builder, List<GraphQLQueryObjectField> fields)
    {
        foreach (var field in fields)
        {
            if (field.HasAliasName())
            {
                builder.Append(field.AliasName).Append(": ");
            }

            if (field.HasFields)
            {
                if (field.Arguments is not null)
                {
                    AppendCamelCase(builder, field.Name);
                    builder.Append(" (");
                    AppendArguments(builder, field.Arguments);
                    builder.AppendLine(") {");
                }
                else
                {
                    AppendCamelCase(builder, field.Name);
                    builder.AppendLine(" {");
                }

                AppendFields(builder, field.Fields);

                builder.AppendLine("}");
            }
            else
            {
                AppendCamelCase(builder, field.Name);
                builder.AppendLine();
            }
        }
    }

    private static void AppendCamelCase(StringBuilder builder, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var first = value[0];

        if (char.IsUpper(first))
        {
            builder.Append(char.ToLowerInvariant(first));

            if (value.Length > 1)
            {
                builder.Append(value, 1, value.Length - 1);
            }
        }
        else
        {
            builder.Append(value);
        }
    }

    private static string FormatQueryArgument(object value)
    {
        return value switch
        {
            bool booleanValue => booleanValue ? "true" : "false",
            string stringValue => JsonSerializer.Serialize(stringValue, ArgumentSerializerOptions),
            Guid guidValue => "\"" + guidValue + "\"",
            DateTime dateTimeValue => "\"" + dateTimeValue.ToString("s", CultureInfo.InvariantCulture) + "\"",
            float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
            double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
            decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            _ => value.ToString()
        };
    }

}
