using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class LootChest
    {
        [JsonProperty(PropertyName = "@uniquename")]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "@faction")]
        public string Faction { get; set; }
    }
}