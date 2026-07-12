using FluentGraphQL.Classes;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class ArgumentUsageTests
{
    [Fact]
    public void Should_Generate_Query_With_Query_And_Where_Arguments()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            eq = "Paris"
                        }
                    }
                }));

        Assert.Equal(
            "query { accounts(where: { city: { eq: \"Paris\" } }) { id societyName } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Query_And_Where_In_Array_Arguments()
    {
        var builder = new GraphQLQueryBuilder();
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            @in = cities
                        }
                    }
                }));

        Assert.Equal(
            "query { accounts(where: { city: { in: [ \"Paris\", \"London\", \"Madrid\", \"New York\" ] } }) { id societyName } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Query_And_Variables()
    {
        var builder = new GraphQLQueryBuilder();
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, cities)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        city = new
                        {
                            @in = Var("cities")
                        }
                    }
                }));

        Assert.Equal(
            "query ($cities: [String]!) { accounts(where: { city: { in: $cities } }) { id societyName } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Query_And_Two_Variables()
    {
        var builder = new GraphQLQueryBuilder();

        var name = "Paul";
        var cities = new string[] { "Paris", "London", "Madrid", "New York" };

        builder
            .AddVariable("firstName", GraphQLParameterType.STRING, name)
            .AddVariable("cities", GraphQLParameterType.STRING_ARRAY, cities)
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .WithArguments(new
                {
                    where = new
                    {
                        and = new object[]
                        {
                            new
                            {
                                city = new
                                {
                                    @in = Var("cities")
                                }
                            },
                            new
                            {
                                contacts = new
                                {
                                    firstName = new
                                    {
                                        eq = Var("firstName")
                                    }
                                }
                            }
                        }
                    }
                }));

        Assert.Equal(
            "query ($firstName: String!, $cities: [String]!) { accounts(where: { and: [ { city: { in: $cities } }, { contacts: { firstName: { eq: $firstName } } } ] }) { id societyName } }",
            Normalize(builder.Query));
    }
}
