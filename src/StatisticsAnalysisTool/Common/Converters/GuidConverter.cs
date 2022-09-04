using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Common.Converters;
public class GuidConverter : JsonConverter<Guid>
{
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N"));
    }

    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() is { } str ? Guid.Parse(str) : default;
    }
}