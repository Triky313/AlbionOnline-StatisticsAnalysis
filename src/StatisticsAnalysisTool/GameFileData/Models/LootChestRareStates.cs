using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestRareStates
{
    [JsonPropertyName("@hiderarityuntil")]
    public string HideRarityUntil { get; set; } = string.Empty;

    [JsonPropertyName("RareState")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootChestRareState>))]
    public List<LootChestRareState> RareState { get; set; } = [];
}
