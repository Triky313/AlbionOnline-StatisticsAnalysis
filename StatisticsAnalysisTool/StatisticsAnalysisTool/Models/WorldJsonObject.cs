using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class WorldJsonObject
    {
        [JsonProperty("Index")]
        public string Index { get; set; }

        [JsonProperty("UniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }
    }
}