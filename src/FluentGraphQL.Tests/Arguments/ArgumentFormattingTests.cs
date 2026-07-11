using System.Globalization;

using FluentGraphQL.Classes;

using static FluentGraphQL.Tests.TestHelpers;

namespace FluentGraphQL.Tests;

public class ArgumentFormattingTests
{
    [Fact]
    public void FormatArgument_StringLiteralContainsDoubleQuote_IsEscaped()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { name = new { eq = "Pa\"ris" } } }));

        Assert.Equal(
            "query { accounts(where: { name: { eq: \"Pa\\\"ris\" } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void FormatArgument_DateTimeLiteral_RendersAsQuotedIso8601RegardlessOfCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        try
        {
            var builder = new GraphQLQueryBuilder();

            builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { createdAt = new { gt = new DateTime(2026, 7, 10, 14, 30, 0) } } }));

            Assert.Equal(
                "query { accounts(where: { createdAt: { gt: \"2026-07-10T14:30:00\" } }) { id } }",
                Normalize(builder.Query));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void FormatArgument_GuidLiteral_IsQuoted()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { id = new { eq = Guid.Parse("550e8400-e29b-41d4-a716-446655440000") } } }));

        Assert.Equal(
            "query { accounts(where: { id: { eq: \"550e8400-e29b-41d4-a716-446655440000\" } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void FormatArgument_NonArrayEnumerable_RendersAsList()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { ids = new { @in = new List<int> { 1, 2, 3 } } } }));

        Assert.Equal(
            "query { accounts(where: { ids: { in: [ 1, 2, 3 ] } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void FormatArgument_EnumLiteral_RendersAsBareName()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { day = new { eq = DayOfWeek.Monday } } }));

        Assert.Equal(
            "query { accounts(where: { day: { eq: Monday } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void FormatArgument_BooleanLiteral_RendersAsLowercaseKeyword()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { active = new { eq = true } } }));

        Assert.Equal(
            "query { accounts(where: { active: { eq: true } }) { id } }",
            Normalize(builder.Query));
    }

    [Fact]
    public void FormatArgument_DecimalLiteral_RendersInvariantRegardlessOfCulture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

        try
        {
            var builder = new GraphQLQueryBuilder();

            builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
                .AddField(account => account.Id)
                .WithArguments(new { where = new { latitude = new { gt = 12.5m } } }));

            Assert.Equal(
                "query { accounts(where: { latitude: { gt: 12.5 } }) { id } }",
                Normalize(builder.Query));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void FormatArgument_DictionaryValue_ThrowsInsteadOfEmittingMalformedList()
    {
        var builder = new GraphQLQueryBuilder();

        builder.AddQuery(new GraphQLQueryObject<Account>("accounts")
            .AddField(account => account.Id)
            .WithArguments(new { where = new { tags = new Dictionary<string, int> { ["a"] = 1 } } }));

        var exception = Record.Exception(() => builder.Query);

        Assert.IsType<InvalidOperationException>(exception);
    }
}
