using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class GameInfoSearchResponse
    {
        [JsonPropertyName("guilds")]
        public List<SearchGuildResponse> SearchGuilds { get; set; }

        [JsonPropertyName("players")]
        public List<SearchPlayerResponse> SearchPlayer { get; set; }
    }
}