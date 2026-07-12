namespace FluentGraphQL;

internal static class GraphQLName
{
    public static string Validate(string name)
    {
        if (IsValid(name) == false)
        {
            throw new ArgumentException($"'{name}' is not a valid GraphQL name (expected [_A-Za-z][_0-9A-Za-z]*).", nameof(name));
        }

        return name;
    }

    private static bool IsValid(string name)
    {
        if (string.IsNullOrEmpty(name) || IsNameStart(name[0]) == false)
        {
            return false;
        }

        for (var index = 1; index < name.Length; index++)
        {
            if (IsNameContinue(name[index]) == false)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNameStart(char character)
    {
        return character == '_' || (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');
    }

    private static bool IsNameContinue(char character)
    {
        return IsNameStart(character) || (character >= '0' && character <= '9');
    }
}
