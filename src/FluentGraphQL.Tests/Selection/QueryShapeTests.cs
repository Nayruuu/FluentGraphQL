using FluentGraphQL.Classes;

using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class QueryShapeTests
{
    [Fact]
    public void Should_Generate_Query_With_Account_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddEveryFields());

        Assert.Equal(
            "query { accounts { id societyName } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Alias()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .As("myAlias")
                .AddEveryFields());

        Assert.Equal(
            "query { myAlias: accounts { id societyName } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Only_Account_Society_Name()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id));

        Assert.Equal(
            "query { accounts { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Contact_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields());

        Assert.Equal(
            "query { contacts { id firstName lastName email phoneNumber } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Contact_Properties_But_Phone_Number_Excluded()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields()
                .Except(contact => contact.PhoneNumber));

        Assert.Equal(
            "query { contacts { id firstName lastName email } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Account_And_Adresse_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .AddField(
                    account => account.Adresse,
                    adresse => adresse.AddEveryFields()));

        Assert.Equal(
            "query { accounts { id societyName adresse { streetNumber zipCode city latitude longitude } } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Account_And_Contacts_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
            .AddCollectionField(
                account => account.Contacts,
                contact => contact.AddEveryFields()));

        Assert.Equal(
            "query { accounts { id societyName contacts { id firstName lastName email phoneNumber } } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Query_With_Account_And_Contacts_And_Tasks_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields()
                .AddCollectionField(
                    account => account.Contacts,
                    contact => contact
                        .AddEveryFields()
                        .AddCollectionField(
                            c => c.Tasks,
                            task => task.AddEveryFields())));

        Assert.Equal(
            "query { accounts { id societyName contacts { id firstName lastName email phoneNumber tasks { id name description startDate dueDate } } } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Should_Generate_Two_Queries_With_Account_And_Contacts_Initial_Properties()
    {
        var builder = new GraphQLQueryBuilder();

        builder
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddEveryFields())
            .AddQuery(new GraphQLQueryObject<Contact>("contacts")
                .AddEveryFields());

        Assert.Equal(
            "query { accounts { id societyName } contacts { id firstName lastName email phoneNumber } }",
            Normalize(builder.Query));
    }
}
