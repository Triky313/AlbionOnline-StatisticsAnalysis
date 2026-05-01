using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootItemJsonObject
{
    [JsonPropertyName("@type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("@amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("@chance")]
    public double Chance { get; set; }

    [JsonPropertyName("@useblackmarket")]
    public string UseBlackMarket { get; set; } = string.Empty;

    [JsonPropertyName("@scalewithgoldeconomy")]
    public string ScaleWithGoldEconomy { get; set; } = string.Empty;

    [JsonPropertyName("@ignoreforactivitychest")]
    public string IgnoreForActivityChest { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = [];
}
