using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestLoot
{
    [JsonPropertyName("LootByTier")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootByTierJsonObject>))]
    public List<LootByTierJsonObject> LootByTier { get; set; } = [];

    [JsonPropertyName("LootListReference")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootListReferenceJsonObject>))]
    public List<LootListReferenceJsonObject> LootListReference { get; set; } = [];
}
