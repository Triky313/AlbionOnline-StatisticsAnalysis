using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Converter;

public class MarkerToMarkersListConverter : JsonConverter<List<Marker>>
{
    public override bool HandleNull => true;

    public override List<Marker> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        return ReadMarkers(jsonDocument.RootElement);
    }

    public override void Write(Utf8JsonWriter writer, List<Marker> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var marker in value)
        {
            writer.WriteStartObject();

            writer.WriteString("@type", marker.Type);

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static List<Marker> ReadMarkers(JsonElement element)
    {
        var markers = new List<Marker>();

        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var markerElement in element.EnumerateArray())
                {
                    AddMarker(markers, markerElement);
                }

                return markers;
            case JsonValueKind.Object:
                AddMarker(markers, element);
                return markers;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return markers;
            default:
                throw new JsonException("Invalid JSON format for Marker");
        }
    }

    private static void AddMarker(List<Marker> markers, JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Invalid JSON format for Marker");
        }

        if (!element.TryGetProperty("@type", out var typeElement))
        {
            return;
        }

        if (typeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Invalid JSON format for Marker");
        }

        markers.Add(new Marker
        {
            Type = typeElement.GetString()
        });
    }
}
