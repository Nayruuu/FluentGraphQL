using FluentGraphQL.Classes;
using FluentGraphQL.Classes.Inputs;

using static FluentGraphQL.GraphQL;

namespace FluentGraphQL.Tests;

public class VariableInferenceTests
{
    [Fact]
    public void AddVariable_TypeInferredFromInt_DeclaresRequiredInt()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("count", 42)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { take = Var("count") }));

        Assert.Contains("$count: Int!", builder.Query);
    }

    [Fact]
    public void AddVariable_TypeInferredFromString_DeclaresRequiredString()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("firstName", "Paul")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { name = new { eq = Var("firstName") } } }));

        Assert.Contains("$firstName: String!", builder.Query);
    }

    [Fact]
    public void AddVariable_TypeInferredFromList_DeclaresArray()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", new List<string> { "Paris" })
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { city = new { @in = Var("cities") } } }));

        Assert.Contains("$cities: [String]!", builder.Query);
    }

    [Fact]
    public void AddVariable_TypeInferredFromStringArray_DeclaresStringArray()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", new[] { "Paris" })
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { city = new { @in = Var("cities") } } }));

        Assert.Contains("$cities: [String]!", builder.Query);
    }

    [Fact]
    public void AddVariable_TypeInferredFromNamedObject_UsesTypeName()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("input", new SaveAccountInput { Account = new Account { SocietyName = "Acme" } })
            .AddQuery(new GraphQLQueryObject<Account>("saveAccount")
                .AddField(account => account.Id)
                .WithArguments(new { input = Var("input") }));

        Assert.Contains("$input: SaveAccountInput", builder.Query);
        Assert.DoesNotContain("AnonymousType", builder.Query);
    }

    [Fact]
    public void AddVariable_EmptyName_ThrowsInsteadOfProducingMalformedOutput()
    {
        var builder = new GraphQLQueryBuilder();

        var exception = Record.Exception(() => builder.AddVariable("", "x"));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void AddVariable_ListValueWithoutExplicitType_InfersArrayTypeNotClrTypeName()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("ids", new List<int> { 1, 2, 3 })
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { id = new { @in = Var("ids") } } }));

        var query = builder.Query;

        Assert.Contains("[Int", query);
        Assert.DoesNotContain("List", query);
    }
}
