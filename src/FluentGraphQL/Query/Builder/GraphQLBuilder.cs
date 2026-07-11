namespace FluentGraphQL;

/// <summary>
/// Base builder holding the declared variables shared by query and mutation builders.
/// </summary>
public abstract class GraphQLBuilder
{
    protected bool HasParameters => _parameters is { Count: > 0 };
    
    private Dictionary<string, GraphQLParameter> _parameters;

    protected Dictionary<string, GraphQLParameter> Parameters => _parameters ??= new Dictionary<string, GraphQLParameter>();
}
