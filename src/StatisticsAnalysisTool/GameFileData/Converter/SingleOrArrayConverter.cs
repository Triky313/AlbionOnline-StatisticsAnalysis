using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Converter;

public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
{
    public override bool HandleNull => true;

    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        return ReadItems(jsonDocument.RootElement, options);
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }

        writer.WriteEndArray();
    }

    private static List<T> ReadItems(JsonElement element, JsonSerializerOptions options)
    {
        var items = new List<T>();

        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var itemElement in element.EnumerateArray())
                {
                    AddItem(items, itemElement, options);
                }

                return items;
            case JsonValueKind.Object:
                AddItem(items, element, options);
                return items;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return items;
            default:
                throw new JsonException("Invalid JSON format for list data.");
        }
    }

    private static void AddItem(List<T> items, JsonElement element, JsonSerializerOptions options)
    {
        var item = element.Deserialize<T>(options);
        if (item is not null)
        {
            items.Add(item);
        }
    }
}