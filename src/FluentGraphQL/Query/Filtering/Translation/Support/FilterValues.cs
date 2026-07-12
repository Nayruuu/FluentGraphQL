using System.Reflection;
using System.Linq.Expressions;

namespace FluentGraphQL;

internal static class FilterValues
{
    public static object Evaluate(Expression expression)
    {
        expression = FilterMembers.Unwrap(expression);

        if (expression is MethodCallExpression call && IsVariableReference(call))
        {
            return new GraphQLVariable((string)EvaluateConstant(call.Arguments[0]));
        }

        return EvaluateConstant(expression);
    }

    public static object CoerceEnum(Expression member, object value)
    {
        var type = FilterMembers.Unwrap(member).Type;
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsEnum && value is not null && value is not GraphQLVariable && value.GetType() != type)
        {
            return Enum.ToObject(type, value);
        }

        return value;
    }

    private static object EvaluateConstant(Expression expression)
    {
        expression = FilterMembers.Unwrap(expression);

        switch (expression)
        {
            case ConstantExpression constant:
                return constant.Value;

            case MemberExpression member:
                var owner = member.Expression is null ? null : EvaluateConstant(member.Expression);
                return member.Member switch
                {
                    FieldInfo field => field.GetValue(owner),
                    PropertyInfo property => property.GetValue(owner),
                    _ => CompileEvaluate(expression)
                };

            default:
                return CompileEvaluate(expression);
        }
    }

    private static object CompileEvaluate(Expression expression)
    {
        var boxed = Expression.Convert(expression, typeof(object));

        Func<object> getter;

        try
        {
            getter = Expression.Lambda<Func<object>>(boxed).Compile();
        }
        catch (InvalidOperationException)
        {
            throw FilterMembers.Unsupported(expression);
        }

        return getter();
    }

    private static bool IsVariableReference(MethodCallExpression call)
    {
        return call.Method.DeclaringType == typeof(GraphQL)
            && call.Method.Name == "Var"
            && call.Method.IsGenericMethod;
    }
}
