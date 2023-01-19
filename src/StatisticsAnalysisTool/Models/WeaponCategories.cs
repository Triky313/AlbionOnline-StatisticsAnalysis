using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class WeaponCategories
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}