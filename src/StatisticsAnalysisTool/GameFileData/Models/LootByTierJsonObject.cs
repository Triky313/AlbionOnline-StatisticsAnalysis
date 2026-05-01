using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootByTierJsonObject
{
    [JsonPropertyName("@tier")]
    public int Tier { get; set; }

    [JsonPropertyName("LootListReference")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootListReferenceJsonObject>))]
    public List<LootListReferenceJsonObject> LootListReference { get; set; } = [];
}
