namespace FluentGraphQL;

/// <summary>
/// Envelope for deserializing a GraphQL response's <c>data</c> field into <typeparamref name="T"/>.
/// </summary>
public class GraphQLRequestData<T>
{
    public T Data { get; set; }
}
