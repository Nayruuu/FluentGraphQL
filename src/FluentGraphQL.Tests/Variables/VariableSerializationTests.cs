using FluentGraphQL.Classes;
using FluentGraphQL.Classes.Inputs;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class VariableSerializationTests
{
    [Fact]
    public void Should_Generate_Variables_With_Query_And_Two_Variables()
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
            "{ \"firstName\": \"Paul\", \"cities\": [ \"Paris\", \"London\", \"Madrid\", \"New York\" ] }",
            Normalize(builder.Variables.ToString()));
    }

    [Fact]
    public void Variables_ObjectVariable_SerializesNestedPropertiesAsCamelCase()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddVariable("input", GraphQLParameterType.OBJECT, new SaveAccountInput
            {
                Account = new Account { SocietyName = "Acme" }
            })
            .AddQuery(new GraphQLQueryObject<Account>("saveAccount").AddField(account => account.Id));

        var variables = builder.Variables.ToJsonString();

        Assert.Contains("\"account\"", variables);
        Assert.Contains("\"societyName\"", variables);
        Assert.DoesNotContain("\"Account\"", variables);
        Assert.DoesNotContain("\"SocietyName\"", variables);
    }
}
