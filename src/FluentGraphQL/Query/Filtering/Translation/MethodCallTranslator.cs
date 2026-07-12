using System.Linq.Expressions;

namespace FluentGraphQL;

internal static class MethodCallTranslator
{
    public static FilterObject Translate(MethodCallExpression call, bool negated)
    {
        var name = call.Method.Name;

        if ((name == "Contains" || name == "StartsWith" || name == "EndsWith")
            && call.Object is not null && FilterMembers.RootsAtParameter(call.Object) && call.Object.Type == typeof(string))
        {
            var op = name switch
            {
                "Contains" => "contains",
                "StartsWith" => "startsWith",
                _ => "endsWith"
            };

            return FilterNodes.Nest(FilterMembers.MemberPath(call.Object), negated ? "n" + op : op, FilterValues.Evaluate(call.Arguments[0]));
        }

        if (name == "Contains")
        {
            var collection = call.Object ?? call.Arguments[0];
            var item = call.Object is not null ? call.Arguments[0] : call.Arguments[1];

            if (FilterMembers.RootsAtParameter(item))
            {
                return FilterNodes.Nest(FilterMembers.MemberPath(item), negated ? "nin" : "in", FilterValues.Evaluate(collection));
            }
        }

        if (name == "Any" || name == "All")
        {
            Expression collection;
            LambdaExpression predicate = null;

            if (call.Object is not null)
            {
                collection = call.Object;
                if (call.Arguments.Count > 0)
                {
                    predicate = (LambdaExpression)call.Arguments[0];
                }
            }
            else
            {
                collection = call.Arguments[0];
                if (call.Arguments.Count > 1)
                {
                    predicate = (LambdaExpression)call.Arguments[1];
                }
            }

            if (name == "Any" && predicate is null)
            {
                return FilterNodes.Nest(FilterMembers.MemberPath(collection), "any", true);
            }

            var op = name == "All" ? "all" : negated ? "none" : "some";

            return FilterNodes.Nest(FilterMembers.MemberPath(collection), op, FilterTranslator.TranslatePredicate(predicate.Body));
        }

        throw FilterMembers.Unsupported(call);
    }
}
