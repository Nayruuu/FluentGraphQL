namespace FluentGraphQL;

internal static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsUpper(value[0]) == false)
        {
            return value;
        }

        return string.Create(value.Length, value, static (span, original) =>
        {
            original.AsSpan().CopyTo(span);

            for (var i = 0; i < span.Length; i++)
            {
                if (i == 1 && char.IsUpper(span[i]) == false)
                {
                    break;
                }

                if (i > 0 && i + 1 < span.Length && char.IsUpper(span[i + 1]) == false)
                {
                    break;
                }

                span[i] = char.ToLowerInvariant(span[i]);
            }
        });
    }
}
