using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class SearchGuildResponse
{
    [JsonPropertyName("Id")] public string Id { get; set; }

    [JsonPropertyName("Name")] public string Name { get; set; }

    [JsonPropertyName("AllianceId")]
    public string AllianceId { get; set; }

    [JsonPropertyName("AllianceName")]
    public string AllianceName { get; set; }

    [JsonPropertyName("KillFame")]
    public ulong? KillFame { get; set; }

    [JsonPropertyName("DeathFame")]
    public ulong? DeathFame { get; set; }
}