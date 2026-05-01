using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestJsonRootObject
{
    [JsonPropertyName("LootChests")]
    public LootChestJsonObjects LootChests { get; set; } = new();
}
