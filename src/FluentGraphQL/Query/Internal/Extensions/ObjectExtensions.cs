namespace FluentGraphQL;

internal static class ObjectExtensions
{
    private static readonly HashSet<Type> PrimitiveTypes = new()
    {
        typeof(string), typeof(char), typeof(byte), typeof(sbyte),
        typeof(ushort), typeof(short), typeof(uint), typeof(int),
        typeof(ulong), typeof(long), typeof(float), typeof(double),
        typeof(decimal), typeof(DateTime), typeof(Guid),

        typeof(char?), typeof(byte?), typeof(sbyte?), typeof(ushort?),
        typeof(short?), typeof(uint?), typeof(int?), typeof(ulong?),
        typeof(long?), typeof(float?), typeof(double?), typeof(decimal?),
        typeof(DateTime?), typeof(Guid?)
    };

    public static bool IsPrimitive(this Type type)
    {
        return type.IsPrimitive || PrimitiveTypes.Contains(type);
    }
}
