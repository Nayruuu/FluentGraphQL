using System.Linq.Expressions;

namespace FluentGraphQL;

internal static class FilterTranslator
{
    public static FilterObject Translate(Expression body)
    {
        return TranslatePredicate(FilterMembers.Unwrap(body));
    }

    public static FilterObject TranslatePredicate(Expression expression)
    {
        switch (FilterMembers.Unwrap(expression))
        {
            case BinaryExpression binary when binary.NodeType == ExpressionType.AndAlso:
                return FilterNodes.Merge(TranslatePredicate(binary.Left), TranslatePredicate(binary.Right));

            case BinaryExpression binary when binary.NodeType == ExpressionType.OrElse:
                return new FilterObject().Add("or", new FilterArray
                {
                    Items = { TranslatePredicate(binary.Left), TranslatePredicate(binary.Right) }
                });

            case BinaryExpression binary:
                return ComparisonTranslator.Translate(binary);

            case MethodCallExpression call:
                return MethodCallTranslator.Translate(call, negated: false);

            case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
                return Negate(FilterMembers.Unwrap(unary.Operand));

            default:
                throw FilterMembers.Unsupported(expression);
        }
    }

    private static FilterObject Negate(Expression operand)
    {
        switch (operand)
        {
            case MethodCallExpression call:
                return MethodCallTranslator.Translate(call, negated: true);

            case BinaryExpression binary when binary.NodeType == ExpressionType.Equal:
                return ComparisonTranslator.WithOperator(binary, "neq");

            case BinaryExpression binary when binary.NodeType == ExpressionType.NotEqual:
                return ComparisonTranslator.WithOperator(binary, "eq");

            default:
                throw FilterMembers.Unsupported(operand);
        }
    }
}
