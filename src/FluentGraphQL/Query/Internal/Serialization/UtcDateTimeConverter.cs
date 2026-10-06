using System.Text.Json;
using System.Globalization;
using System.Text.Json.Serialization;

namespace FluentGraphQL;

internal sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public static string Format(DateTime value)
    {
        return value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Format(value));
    }
}
