using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Converter;

public class FlexibleDoubleJsonConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String => ParseString(reader.GetString()),
            JsonTokenType.Null => 0,
            _ => throw new JsonException($"Cannot convert JSON token '{reader.TokenType}' to double.")
        };
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }

    private static double ParseString(string value)
    {
        var trimmedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return 0;
        }

        if (double.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new JsonException($"Cannot convert JSON string '{value}' to double.");
    }
}
