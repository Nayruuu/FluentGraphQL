using System.Linq.Expressions;

namespace FluentGraphQL;

internal static class ComparisonTranslator
{
    public static FilterObject Translate(BinaryExpression binary)
    {
        return WithOperator(binary, OperatorFor(binary));
    }

    public static FilterObject WithOperator(BinaryExpression binary, string op)
    {
        var (member, valueExpression) = FilterMembers.SplitMemberValue(binary.Left, binary.Right);

        return FilterNodes.Nest(
            FilterMembers.MemberPath(member),
            op,
            FilterValues.CoerceEnum(member, FilterValues.Evaluate(valueExpression)));
    }

    private static string OperatorFor(BinaryExpression binary)
    {
        return binary.NodeType switch
        {
            ExpressionType.Equal => "eq",
            ExpressionType.NotEqual => "neq",
            ExpressionType.GreaterThan => "gt",
            ExpressionType.GreaterThanOrEqual => "gte",
            ExpressionType.LessThan => "lt",
            ExpressionType.LessThanOrEqual => "lte",
            _ => throw FilterMembers.Unsupported(binary)
        };
    }
}
