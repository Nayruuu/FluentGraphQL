namespace FluentGraphQL;

internal static class GraphQLTypeName
{
    private static readonly Dictionary<Type, string> ScalarNames = new()
    {
        [typeof(string)] = "String",
        [typeof(bool)] = "Boolean",
        [typeof(Guid)] = "UUID",
        [typeof(DateTime)] = "DateTime",
        [typeof(int)] = "Int",
        [typeof(short)] = "Int",
        [typeof(byte)] = "Int",
        [typeof(long)] = "Long",
        [typeof(float)] = "Float",
        [typeof(double)] = "Float",
        [typeof(decimal)] = "Decimal"
    };

    public static string Resolve(GraphQLParameter parameter)
    {
        return parameter.Type is { } type
            ? FromParameterType(type, parameter.Value)
            : FromClrType(parameter.Value.GetType());
    }

    private static string FromParameterType(GraphQLParameterType type, object value)
    {
        return type switch
        {
            GraphQLParameterType.INT => "Int!",
            GraphQLParameterType.STRING => "String!",
            GraphQLParameterType.DATETIME => "DateTime!",
            GraphQLParameterType.BOOLEAN => "Boolean!",
            GraphQLParameterType.STRING_ARRAY => "[String]!",
            GraphQLParameterType.INT_ARRAY => "[Int!]!",
            GraphQLParameterType.DATETIME_ARRAY => "[DateTime!]!",
            GraphQLParameterType.OBJECT => value.GetType().Name,
            GraphQLParameterType.UUID => "UUID!",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported GraphQL parameter type.")
        };
    }

    private static string FromClrType(Type type)
    {
        var element = ElementType(type);

        if (element is not null)
        {
            var elementName = element.IsValueType ? Scalar(element) + "!" : Scalar(element);

            return "[" + elementName + "]!";
        }

        return Scalar(type) + "!";
    }

    private static Type ElementType(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        foreach (var contract in type.GetInterfaces())
        {
            if (contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return contract.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private static string Scalar(Type type)
    {
        return ScalarNames.TryGetValue(type, out var name) ? name : type.Name;
    }
}
