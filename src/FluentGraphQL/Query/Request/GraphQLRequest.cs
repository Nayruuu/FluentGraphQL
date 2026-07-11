using System.Text.Json.Nodes;

namespace FluentGraphQL;

/// <summary>
/// A GraphQL request payload: the query document and its variables.
/// </summary>
public class GraphQLRequest
{
    public string Query { get; set; }

    public JsonObject Variables { get; set; }

    public GraphQLRequest()
    {
    }

    public GraphQLRequest(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or whitespace.", nameof(query));
        }

        Query = query;
    }

    public GraphQLRequest(string query, JsonObject variables) : this(query)
    {
        Variables = variables;
    }
}
