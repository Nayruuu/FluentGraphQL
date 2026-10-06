namespace FluentGraphQL;

/// <summary>
/// Envelope for deserializing a GraphQL response: its <c>data</c> field into <typeparamref name="T"/>, and its <c>errors</c>.
/// </summary>
public class GraphQLRequestData<T>
{
    public T Data { get; set; }

    public IList<GraphQLResponseError> Errors { get; set; }
}
