using StatisticsAnalysisTool.GameFileData.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Converter;

public class MarkerToMarkersListConverter : JsonConverter<List<Marker>>
{
    public override List<Marker> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var markers = new List<Marker>();

        try
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return ReadArray(ref reader);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return ReadObject(ref reader);
            }

            throw new JsonException("Invalid JSON format for Marker");
        }
        catch (Exception)
        {
            return markers;
        }
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

    private List<Marker> ReadArray(ref Utf8JsonReader reader)
    {
        var markers = new List<Marker>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return markers;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var marker = ReadMarker(ref reader);
                markers.Add(marker);
            }
        }

        throw new JsonException("Invalid JSON format for Marker");
    }

    private Marker ReadMarker(ref Utf8JsonReader reader)
    {
        var marker = new Marker();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return marker;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                if (propertyName == "@type")
                {
                    reader.Read();
                    marker.Type = reader.GetString();
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        throw new JsonException("Invalid JSON format for Marker");
    }

    private List<Marker> ReadObject(ref Utf8JsonReader reader)
    {
        var markers = new List<Marker>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return markers;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                if (propertyName == "@type")
                {
                    reader.Read();
                    var type = reader.GetString();
                    var marker = new Marker { Type = type };
                    markers.Add(marker);
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        throw new JsonException("Invalid JSON format for Marker");
    }
}