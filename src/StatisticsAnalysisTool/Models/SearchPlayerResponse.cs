using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class SearchPlayerResponse
{
    [JsonPropertyName("Id")] 
    public string Id { get; set; }

    [JsonPropertyName("Name")] 
    public string Name { get; set; }

    [JsonPropertyName("GuildId")]
    public string GuildId { get; set; }

    [JsonPropertyName("GuildName")]
    public string GuildName { get; set; }

    [JsonPropertyName("AllianceId")]
    public string AllianceId { get; set; }

    [JsonPropertyName("AllianceName")]
    public string AllianceName { get; set; }

    [JsonPropertyName("Avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("AvatarRing")]
    public string AvatarRing { get; set; }

    [JsonPropertyName("KillFame")]
    public ulong? KillFame { get; set; }

    [JsonPropertyName("DeathFame")]
    public ulong? DeathFame { get; set; }

    [JsonPropertyName("FameRatio")]
    public double? FameRatio { get; set; }

    [JsonPropertyName("totalKills")]
    public ulong? TotalKills { get; set; }

    [JsonPropertyName("gvgKills")]
    public ulong? GvgKills { get; set; }

    [JsonPropertyName("gvgWon")]
    public ulong? GvgWon { get; set; }
}