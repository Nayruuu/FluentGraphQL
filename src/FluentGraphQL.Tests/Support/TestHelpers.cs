using System.Text.RegularExpressions;

namespace FluentGraphQL.Tests;

public static class TestHelpers
{
    public static string Normalize(string query)
    {
        var normalized = Regex.Replace(query, @"\n\t*", " ").Trim();

        normalized = Regex.Replace(normalized, @"\t+", "").Trim();
        normalized = Regex.Replace(normalized, @" {2,}", " ");

        return normalized;
    }
}
