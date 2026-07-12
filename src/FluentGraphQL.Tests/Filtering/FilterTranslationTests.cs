using FluentGraphQL.Classes;

using static FluentGraphQL.GraphQL;
using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class FilterTranslationTests
{
    [Fact]
    public void Equality_RendersEq()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "Acme"));

        Assert.Equal(
            "query { accounts(where: { societyName: { eq: \"Acme\" } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void Inequality_RendersNeq()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName != "Acme"));

        Assert.Contains("societyName: { neq: \"Acme\" }", builder.Query);
    }

    [Fact]
    public void GreaterThanFamily_RendersComparisonOperators()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<FluentGraphQL.Classes.Task>("tasks")
            .AddField(t => t.Id)
            .Where(x => x.Id >= 5));

        Assert.Contains("id: { gte: 5 }", builder.Query);
    }

    [Fact]
    public void EqualityWithNull_RendersEqNull()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == null));

        Assert.Contains("societyName: { eq: null }", builder.Query);
    }

    [Fact]
    public void StringContains_RendersContains()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName.Contains("Ac")));

        Assert.Contains("societyName: { contains: \"Ac\" }", builder.Query);
    }

    [Fact]
    public void NegatedStringContains_RendersNcontains()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => !x.SocietyName.StartsWith("Ac")));

        Assert.Contains("societyName: { nstartsWith: \"Ac\" }", builder.Query);
    }

    [Fact]
    public void AndAlso_MergesSiblingFields()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "Acme" && x.Adresse.City == "Paris"));

        Assert.Equal(
            "query { accounts(where: { societyName: { eq: \"Acme\" }, adresse: { city: { eq: \"Paris\" } } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void OrElse_RendersOrArray()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "A" || x.SocietyName == "B"));

        Assert.Equal(
            "query { accounts(where: { or: [ { societyName: { eq: \"A\" } }, { societyName: { eq: \"B\" } } ] }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void ListContains_RendersIn()
    {
        var cities = new[] { "Paris", "London" };
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => cities.Contains(x.SocietyName)));

        Assert.Contains("societyName: { in: [ \"Paris\", \"London\" ] }", builder.Query);
    }

    [Fact]
    public void NegatedListContains_RendersNin()
    {
        var cities = new[] { "Paris" };
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => !cities.Contains(x.SocietyName)));

        Assert.Contains("societyName: { nin: [ \"Paris\" ] }", builder.Query);
    }

    [Fact]
    public void CollectionAnyWithPredicate_RendersSome()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.Contacts.Any(c => c.FirstName == "John")));

        Assert.Equal(
            "query { accounts(where: { contacts: { some: { firstName: { eq: \"John\" } } } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void CollectionAll_RendersAll()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.Contacts.All(c => c.Email == "x@y.z")));

        Assert.Contains("contacts: { all: { email: { eq: \"x@y.z\" } } }", builder.Query);
    }

    [Fact]
    public void NegatedCollectionAny_RendersNone()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => !x.Contacts.Any(c => c.FirstName == "John")));

        Assert.Contains("contacts: { none: { firstName: { eq: \"John\" } } }", builder.Query);
    }

    [Fact]
    public void CollectionAnyWithoutPredicate_RendersAnyTrue()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.Contacts.Any()));

        Assert.Contains("contacts: { any: true }", builder.Query);
    }

    [Fact]
    public void NestedObject_RendersBareNesting()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.Adresse.City == "Paris"));

        Assert.Contains("adresse: { city: { eq: \"Paris\" } }", builder.Query);
    }

    [Fact]
    public void VariableReference_RendersDollarName()
    {
        var builder = new GraphQLQueryBuilder();
        builder
            .AddVariable("name", "Acme")
            .AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(a => a.Id)
                .Where(x => x.SocietyName == Var<string>("name")));

        Assert.Equal(
            "query ($name: String!) { accounts(where: { societyName: { eq: $name } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void EnumValue_RendersBareName()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Widget>("widgets")
            .AddField(w => w.Id)
            .Where(x => x.Status == WidgetStatus.Active));

        Assert.Equal(
            "query { widgets(where: { status: { eq: Active } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void CapturedLocal_RendersItsRuntimeValue()
    {
        var name = "Acme";
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == name));

        Assert.Contains("societyName: { eq: \"Acme\" }", builder.Query);
    }

    [Fact]
    public void RepeatedWhere_CombinesWithImplicitAnd()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "Acme")
            .Where(x => x.Adresse.City == "Paris"));

        Assert.Equal(
            "query { accounts(where: { societyName: { eq: \"Acme\" }, adresse: { city: { eq: \"Paris\" } } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void RepeatedWhere_SameField_FallsBackToAndArray()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(a => a.Id)
            .Where(x => x.SocietyName == "A")
            .Where(x => x.SocietyName == "B"));

        Assert.Contains(
            "and: [ { societyName: { eq: \"A\" } }, { societyName: { eq: \"B\" } } ]",
            builder.Query);
    }

    [Fact]
    public void MemberToMemberComparison_ThrowsActionableException()
    {
        var exception = Record.Exception(() =>
            new GraphQLQueryObject<Account>("accounts").Where(x => x.SocietyName == x.Adresse.City));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("WithArguments", exception.Message);
    }

    [Fact]
    public void CorrelatedSubPredicate_ThrowsActionableException()
    {
        var exception = Record.Exception(() =>
            new GraphQLQueryObject<Account>("accounts")
                .Where(x => x.Contacts.Any(c => c.FirstName == x.SocietyName)));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("WithArguments", exception.Message);
    }
}
