using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootOrJsonObject
{
    [JsonPropertyName("@weight")]
    [JsonConverter(typeof(FlexibleDoubleJsonConverter))]
    public double Weight { get; set; }

    [JsonPropertyName("@chance")]
    [JsonConverter(typeof(FlexibleDoubleJsonConverter))]
    public double Chance { get; set; }

    [JsonPropertyName("Item")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootItemJsonObject>))]
    public List<LootItemJsonObject> Item { get; set; } = [];

    [JsonPropertyName("LootListReference")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootListReferenceJsonObject>))]
    public List<LootListReferenceJsonObject> LootListReference { get; set; } = [];

    [JsonPropertyName("OR")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootOrJsonObject>))]
    public List<LootOrJsonObject> Or { get; set; } = [];
}
