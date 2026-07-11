namespace FluentGraphQL;

/// <summary>
/// Explicit GraphQL type of a variable, used by the <c>AddVariable(name, type, value)</c> overload.
/// </summary>
public enum GraphQLParameterType
{
    INT,
    STRING,
    DATETIME,
    BOOLEAN,
    STRING_ARRAY,
    INT_ARRAY,
    DATETIME_ARRAY,
    OBJECT,
    UUID
}
