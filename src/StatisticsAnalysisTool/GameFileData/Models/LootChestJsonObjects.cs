using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestJsonObjects
{
    [JsonPropertyName("LootChest")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootChestJsonObject>))]
    public List<LootChestJsonObject> LootChest { get; set; } = [];
}
