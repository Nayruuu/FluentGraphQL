using System.Linq.Expressions;

namespace FluentGraphQL;

internal static class FilterMembers
{
    public static Expression Unwrap(Expression expression)
    {
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }

    public static bool RootsAtParameter(Expression expression)
    {
        expression = Unwrap(expression);

        while (expression is MemberExpression member)
        {
            expression = Unwrap(member.Expression);
        }

        return expression is ParameterExpression;
    }

    public static (Expression member, Expression value) SplitMemberValue(Expression left, Expression right)
    {
        if (RootsAtParameter(left))
        {
            return (left, right);
        }

        if (RootsAtParameter(right))
        {
            return (right, left);
        }

        throw new InvalidOperationException(
            "A filter comparison must have a member of the filtered type on one side (e.g. x.Name == value).");
    }

    public static string[] MemberPath(Expression expression)
    {
        expression = Unwrap(expression);
        var segments = new Stack<string>();

        while (expression is MemberExpression member)
        {
            segments.Push(member.Member.Name);
            expression = Unwrap(member.Expression);
        }

        if (expression is not ParameterExpression)
        {
            throw Unsupported(expression);
        }

        return segments.ToArray();
    }

    public static InvalidOperationException Unsupported(Expression expression)
    {
        return new InvalidOperationException(
            $"The filter expression '{expression}' is not supported. Use WithArguments(...) for filters this fluent form can't express.");
    }
}
