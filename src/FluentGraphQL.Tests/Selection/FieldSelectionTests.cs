using FluentGraphQL.Classes;

using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class FieldSelectionTests
{
    [Fact]
    public void AddEveryFields_RootTypeWithEnumProperty_IncludesTheEnumField()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Widget>("widgets").AddEveryFields());

        Assert.Equal(
            "query { widgets { id status } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void AddField_SelectorWithBoxingConversion_SelectsTheMemberInsteadOfThrowing()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts").AddField<object>(account => account.Id));

        Assert.Equal(
            "query { accounts { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Except_RemovesTheSelectedField()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Contact>("contacts")
            .AddEveryFields()
            .Except(contact => contact.PhoneNumber));

        Assert.Equal(
            "query { contacts { id firstName lastName email } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Except_OnNestedCollectionField_RemovesTheSelectedSubField()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .AddCollectionField(
                account => account.Contacts,
                contact => contact.AddEveryFields().Except(c => c.PhoneNumber)));

        Assert.Equal(
            "query { accounts { id contacts { id firstName lastName email } } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void AddEveryFields_TimeSpanAndNullableBooleanProperties_AreSelected()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Horaire>("horaires").AddEveryFields());

        Assert.Equal("query { horaires { heureDebut heureFin actif } }", Normalize(builder.Query));
    }
}
