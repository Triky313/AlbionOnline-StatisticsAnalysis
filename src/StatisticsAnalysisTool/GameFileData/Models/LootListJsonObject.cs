using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootListJsonObject
{
    [JsonPropertyName("@name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("@listtype")]
    public string ListType { get; set; } = string.Empty;

    [JsonPropertyName("Item")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootItemJsonObject>))]
    public List<LootItemJsonObject> Item { get; set; } = [];

    [JsonPropertyName("LootListReference")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootListReferenceJsonObject>))]
    public List<LootListReferenceJsonObject> LootListReference { get; set; } = [];

    [JsonPropertyName("OR")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootOrJsonObject>))]
    public List<LootOrJsonObject> Or { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = [];
}
