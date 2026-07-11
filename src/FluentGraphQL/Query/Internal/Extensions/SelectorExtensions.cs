using System.Collections.Concurrent;

namespace FluentGraphQL;

internal static class SelectorExtensions
{
    private static readonly ConcurrentDictionary<string, string> MemberNameCache = new();

    public static string ToMemberName(this string selectorText)
    {
        if (string.IsNullOrEmpty(selectorText))
        {
            throw new ArgumentException("The member selector could not be captured.", nameof(selectorText));
        }

        if (MemberNameCache.TryGetValue(selectorText, out var name))
        {
            return name;
        }

        return MemberNameCache.GetOrAdd(selectorText, static text => ParseMemberName(text));
    }

    private static string ParseMemberName(string selectorText)
    {
        var start = selectorText.LastIndexOf('.') + 1;
        var end = start;

        while (end < selectorText.Length && (char.IsLetterOrDigit(selectorText[end]) || selectorText[end] == '_'))
        {
            end++;
        }

        if (end == start)
        {
            throw new ArgumentException($"'{selectorText}' is not a member access expression.", nameof(selectorText));
        }

        return selectorText.Substring(start, end - start);
    }
}
