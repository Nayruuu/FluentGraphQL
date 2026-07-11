using FluentGraphQL.Classes;
using FluentGraphQL.Classes.Inputs;

using GraphQLParser;

using static FluentGraphQL.GraphQL;

namespace FluentGraphQL.Tests;

// The safety net the suite lacked: string assertions can enshrine invalid output
// (an empty "()" looked fine as a literal but is rejected by every conformant server).
// Every generated query is parsed with a real, spec-conformant GraphQL parser.
public class GraphQLValidityTests
{
    [Fact]
    public void ArgumentLessQuery_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts").AddEveryFields());

        AssertValid(builder.Query);
    }

    [Fact]
    public void NestedCollections_AreValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddEveryFields()
            .AddCollectionField(account => account.Contacts, contact => contact
                .AddEveryFields()
                .AddCollectionField(c => c.Tasks, task => task.AddEveryFields())));

        AssertValid(builder.Query);
    }

    [Fact]
    public void GuidArgument_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { id = new { eq = Guid.NewGuid() } } }));

        AssertValid(builder.Query);
    }

    [Fact]
    public void ListArgument_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { ids = new { @in = new List<int> { 1, 2, 3 } } } }));

        AssertValid(builder.Query);
    }

    [Fact]
    public void Mutation_WithObjectVariable_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder(mutation: true);
        builder
            .AddVariable("input", GraphQLParameterType.OBJECT, new SaveAccountInput { Account = new Account { SocietyName = "Acme" } })
            .AddQuery(new GraphQLQueryObject<Account>("saveAccount")
                .WithArguments(new { input = Var("input") })
                .AddField(account => account.Id));

        AssertValid(builder.Query);
    }

    [Fact]
    public void EnumArgument_IsValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { day = new { eq = DayOfWeek.Monday } } }));

        AssertValid(builder.Query);
    }

    [Fact]
    public void FieldLevelArguments_AreValidGraphQL()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .AddCollectionField(account => account.Contacts, contact => contact
                .AddField(
                    c => c.Tasks,
                    new { where = new { name = new { eq = "Onboarding" } } },
                    task => task.AddField(t => t.Id))));

        AssertValid(builder.Query);
    }

    private static void AssertValid(string query)
    {
        var exception = Record.Exception(() => Parser.Parse(query));

        Assert.Null(exception);
    }
}
