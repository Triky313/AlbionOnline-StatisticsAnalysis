using StatisticsAnalysisTool.GameFileData.Converter;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestRareState
{
    [JsonPropertyName("@state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("@weight")]
    [JsonConverter(typeof(FlexibleDoubleJsonConverter))]
    public double Weight { get; set; }

    [JsonPropertyName("@spawnsoundevent")]
    public string SpawnSoundEvent { get; set; } = string.Empty;

    [JsonPropertyName("@opensoundevent")]
    public string OpenSoundEvent { get; set; } = string.Empty;

    [JsonPropertyName("@namelocatag")]
    public string NameLocatag { get; set; } = string.Empty;

    [JsonPropertyName("Loot")]
    public LootChestLoot Loot { get; set; } = new();
}
