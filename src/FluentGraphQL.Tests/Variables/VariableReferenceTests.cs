using FluentGraphQL.Classes;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class VariableReferenceTests
{
    [Fact]
    public void Var_ArgumentValueIsAVariableReference_RendersAsDollarName()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, new[] { "Paris" })
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { city = new { @in = Var("cities") } } }));

        Assert.Equal(
            "query ($cities: [String]!) { accounts(where: { city: { in: $cities } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Var_ReferencesUndeclaredVariable_Throws()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { city = new { eq = Var("cities") } } }));

        var exception = Record.Exception(() => builder.Query);

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Var_ReferencesVariableDeclaredWithNullValue_Throws()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("city", GraphQLParameterType.STRING, null)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { city = new { eq = Var("city") } } }));

        var exception = Record.Exception(() => builder.Query);

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void FormatArgument_StringLiteralEqualsDeclaredVariableName_RendersAsLiteralNotReference()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", GraphQLParameterType.STRING, "x")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { name = new { eq = "cities" } } }));

        Assert.Equal(
            "query ($cities: String!) { accounts(where: { name: { eq: \"cities\" } }) { id } }",
            Normalize(builder.Query));
    }
}
