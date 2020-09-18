namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class GameInfoSearchResponse
    {
        [JsonProperty(PropertyName = "guilds")]
        public List<SearchGuildResponse> SearchGuilds { get; set; }

        [JsonProperty(PropertyName = "players")]
        public List<SearchPlayerResponse> SearchPlayer { get; set; }
    }

    public class SearchGuildResponse
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "AllianceId")]
        public string AllianceId { get; set; }

        [JsonProperty(PropertyName = "AllianceName")]
        public string AllianceName { get; set; }

        [JsonProperty(PropertyName = "KillFame")]
        public ulong? KillFame { get; set; }

        [JsonProperty(PropertyName = "DeathFame")]
        public ulong? DeathFame { get; set; }
    }

    public class SearchPlayerResponse
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "GuildId")]
        public string GuildId { get; set; }

        [JsonProperty(PropertyName = "GuildName")]
        public string GuildName { get; set; }

        [JsonProperty(PropertyName = "AllianceId")]
        public string AllianceId { get; set; }

        [JsonProperty(PropertyName = "AllianceName")]
        public string AllianceName { get; set; }

        [JsonProperty(PropertyName = "Avatar")]
        public string Avatar { get; set; }

        [JsonProperty(PropertyName = "AvatarRing")]
        public string AvatarRing { get; set; }

        [JsonProperty(PropertyName = "KillFame")]
        public ulong? KillFame { get; set; }

        [JsonProperty(PropertyName = "DeathFame")]
        public ulong? DeathFame { get; set; }

        [JsonProperty(PropertyName = "FameRatio")]
        public double? FameRatio { get; set; }

        [JsonProperty(PropertyName = "totalKills")]
        public ulong? TotalKills { get; set; }

        [JsonProperty(PropertyName = "gvgKills")]
        public ulong? GvgKills { get; set; }

        [JsonProperty(PropertyName = "gvgWon")]
        public ulong? GvgWon { get; set; }
    }
}