using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class ShopSubCategory3
{
    [JsonPropertyName("@id")]
    public string Id { get; set; }
    [JsonPropertyName("@value")]
    public string Value { get; set; }
}