using FluentGraphQL.Classes;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class OptionalVariableTests
{
    [Fact]
    public void OptionalVar_WithValue_IsDeclaredAndReferenced()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, new[] { "Paris" })
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .WithArguments(new { where = new { city = new { @in = OptionalVar("cities") } } })
                .AddField(account => account.Id));

        Assert.Equal(
            "query ($cities: [String]!) { accounts(where: { city: { in: $cities } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void OptionalVar_WithoutValue_OmitsTheFilterEntryAndTheDeclaration()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, null)
            .AddVariable("name", GraphQLParameterType.STRING, "Acme")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .WithArguments(new
                {
                    where = new
                    {
                        city = new { @in = OptionalVar("cities") },
                        societyName = new { eq = Var("name") }
                    }
                })
                .AddField(account => account.Id));

        Assert.Equal(
            "query ($name: String!) { accounts(where: { city: { }, societyName: { eq: $name } }) { id } }",
            Normalize(builder.Query));
        Assert.Equal("{\"name\":\"Acme\"}", builder.Variables.ToJsonString());
    }

    [Fact]
    public void OptionalVar_NeverDeclared_OmitsTheRootArgument()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .WithArguments(new { searchFilter = OptionalVar("searchFilter"), first = 10 })
            .AddField(account => account.Id));

        Assert.Equal("query { accounts(first: 10) { id } }", Normalize(builder.Query));
    }

    [Fact]
    public void OptionalVar_OnlyArgumentWithoutValue_DropsTheArgumentList()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("searchFilter", GraphQLParameterType.STRING, null)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .WithArguments(new { searchFilter = OptionalVar("searchFilter") })
                .AddField(account => account.Id));

        Assert.Equal("query { accounts { id } }", Normalize(builder.Query));
    }

    [Fact]
    public void OptionalVar_InListWithoutValue_DropsTheItem()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("a", GraphQLParameterType.STRING, "x")
            .AddVariable("b", GraphQLParameterType.STRING, null)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .WithArguments(new { names = new object[] { OptionalVar("a"), OptionalVar("b") } })
                .AddField(account => account.Id));

        Assert.Equal("query ($a: String!) { accounts(names: [ $a ]) { id } }", Normalize(builder.Query));
    }

    [Fact]
    public void Var_WithoutValue_StillFails()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("name", GraphQLParameterType.STRING, null)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .WithArguments(new { name = Var("name") })
                .AddField(account => account.Id));

        Assert.Throws<InvalidOperationException>(() => builder.Query);
    }
}
