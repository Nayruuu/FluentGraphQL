using System.Reflection;
using System.Collections.Concurrent;

namespace FluentGraphQL;

internal static class TypeProperties
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();

    public static PropertyInfo[] Of(Type type)
    {
        return Cache.GetOrAdd(type, static resolved => resolved.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }
}
