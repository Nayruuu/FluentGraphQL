namespace FluentGraphQL;

internal static class FieldCollectionExtensions
{
    public static void Upsert(this List<GraphQLQueryObjectField> fields, GraphQLQueryObjectField field)
    {
        var key = field.GetPrincipalKey();

        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].GetPrincipalKey() == key)
            {
                fields[i] = field;

                return;
            }
        }

        fields.Add(field);
    }

    public static void RemoveByKey(this List<GraphQLQueryObjectField> fields, string key)
    {
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].GetPrincipalKey() == key)
            {
                fields.RemoveAt(i);

                return;
            }
        }
    }
}
