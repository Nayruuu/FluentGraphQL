using System.Collections.Concurrent;

using FluentGraphQL.Classes;

namespace FluentGraphQL.Tests;

public class ThreadSafetyTests
{
    [Fact]
    public void Query_ReadConcurrentlyFromManyThreads_AlwaysReturnsTheSameResult()
    {
        var builder = new GraphQLQueryBuilder();
        builder.AddQuery(new GraphQLQueryObject<Account>("accounts").AddEveryFields());
        var expected = builder.Query;

        var results = new ConcurrentBag<string>();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, 500, _ =>
        {
            try
            {
                results.Add(builder.Query);
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
        });

        Assert.Empty(exceptions);
        Assert.All(results, result => Assert.Equal(expected, result));
    }
}
