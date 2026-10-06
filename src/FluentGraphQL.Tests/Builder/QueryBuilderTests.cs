using FluentGraphQL.Classes;

using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class QueryBuilderTests
{
    [Fact]
    public void AddQuery_TwoQueriesWithTheSameName_ThrowsInsteadOfSilentlyOverwriting()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts").AddField(account => account.Id));

        var exception = Record.Exception(() =>
            builder.AddQuery(new GraphQLQueryObject<Account>("accounts").AddField(account => account.SocietyName)));

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void AddQuery_SameNameDifferentAliases_AreBothKept()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts").As("first").AddField(account => account.Id))
            .AddQuery(new GraphQLQueryObject<Account>("accounts").As("second").AddField(account => account.SocietyName));

        Assert.Equal(2, builder.QueriesCount);
    }

    [Fact]
    public void BuildQuery_VariableWithNullValue_DoesNotEmitEmptyDeclarationParentheses()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, null)
            .AddQuery(new GraphQLQueryObject<Account>("accounts").AddField(account => account.Id));

        Assert.Equal(
            "query { accounts { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void As_NullAlias_DoesNotThrow()
    {
        var exception = Record.Exception(() => new GraphQLQueryObject<Account>("accounts").As(null));

        Assert.Null(exception);
    }

    [Fact]
    public void As_HostileAlias_ThrowsInsteadOfInjecting()
    {
        var exception = Record.Exception(() =>
            new GraphQLQueryObject<Account>("accounts").As("leaked { id } mine"));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Request_SerializedWithoutNamingPolicy_UsesGraphQLOverHttpFieldNames()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("id", GraphQLParameterType.INT, 42)
            .AddQuery(new GraphQLQueryObject<Account>("accounts").AddField(account => account.Id));

        var json = System.Text.Json.JsonSerializer.Serialize(builder.Request);

        Assert.StartsWith("{\"query\":", json);
        Assert.Contains("\"variables\":{\"id\":42}", json);
    }

    [Fact]
    public void RequestData_GraphQLResponseWithErrors_DeserializesDataAndErrorMessages()
    {
        var json = "{\"data\":{\"count\":3},\"errors\":[{\"message\":\"Champ inconnu\"}]}";

        var response = System.Text.Json.JsonSerializer.Deserialize<GraphQLRequestData<Dictionary<string, int>>>(
            json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal(3, response.Data["count"]);
        Assert.Equal("Champ inconnu", Assert.Single(response.Errors).Message);
    }
}
