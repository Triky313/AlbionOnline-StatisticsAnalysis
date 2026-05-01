using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootJsonRootObject
{
    [JsonPropertyName("LootDefinition")]
    public LootDefinitionJsonObject LootDefinition { get; set; } = new();
}
