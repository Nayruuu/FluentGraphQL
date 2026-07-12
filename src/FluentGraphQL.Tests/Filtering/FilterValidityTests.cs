using FluentGraphQL.Classes;

using GraphQLParser;

using static FluentGraphQL.GraphQL;

namespace FluentGraphQL.Tests;

public class FilterValidityTests
{
    [Fact]
    public void ComparisonAndLogicalFilter_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "Acme"
                && (x.Adresse.City == "Paris" || x.Adresse.City == "London")));

        AssertValid(builder.Query);
    }

    [Fact]
    public void CollectionAndInFilter_IsValidGraphQL()
    {
        var cities = new[] { "Paris", "London" };
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => cities.Contains(x.SocietyName)
                && x.Contacts.Any(c => c.FirstName.Contains("Jo"))));

        AssertValid(builder.Query);
    }

    [Fact]
    public void VariableInFilter_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder
            .AddVariable("name", "Acme")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(a => a.Id)
                .Where(x => x.SocietyName == Var<string>("name")));

        AssertValid(builder.Query);
    }

    [Fact]
    public void ReadmeQuickStartExample_IsValidGraphQL()
    {
        var cities = new[] { "Paris", "London" };
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("firstName", "Paul")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .AddCollectionField(
                    account => account.Contacts,
                    contact => contact
                        .AddEveryFields()
                        .AddCollectionField(
                            c => c.Tasks,
                            task => task.AddEveryFields()))
                .Where(account =>
                    cities.Contains(account.Adresse.City)
                    && account.Contacts.Any(c => c.FirstName == Var<string>("firstName"))));

        AssertValid(builder.Query);
    }

    private static void AssertValid(string query)
    {
        var exception = Record.Exception(() => Parser.Parse(query));

        Assert.Null(exception);
    }
}
