using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Converter;

public class FlexibleIntJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => ParseString(reader.GetString()),
            JsonTokenType.Null => 0,
            _ => throw new JsonException($"Cannot convert JSON token '{reader.TokenType}' to int.")
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }

    private static int ParseString(string value)
    {
        var trimmedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return 0;
        }

        if (int.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new JsonException($"Cannot convert JSON string '{value}' to int.");
    }
}
