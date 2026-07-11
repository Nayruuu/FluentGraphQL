using FluentGraphQL.Classes;
using FluentGraphQL.Classes.Inputs;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class MutationTests
{
    [Fact]
    public void Should_Generate_Mutation()
    {
        var builder = new GraphQLQueryBuilder(mutation: true);

        var saveAccountInput = new SaveAccountInput()
        {
            Account = new Account()
            {
                SocietyName = "MyBeautifulSociety",
                Contacts = new List<Contact>()
                {
                    new Contact()
                    {
                        FirstName = "John",
                        LastName = "Paul"
                    }
                }
            }
        };

        builder
            .AddVariable("input", GraphQLParameterType.OBJECT, saveAccountInput)
            .AddQuery(new GraphQLQueryObject<Account>("saveAccount")
                .WithArguments(new { input = Var("input") })
                .AddField(account => account.Id));

        Assert.Equal(
            "mutation ($input: SaveAccountInput) { saveAccount(input: $input) { id } }",
            Normalize(builder.Query));
    }
}
