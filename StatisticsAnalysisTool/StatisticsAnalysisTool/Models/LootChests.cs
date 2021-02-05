using Newtonsoft.Json;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models
{
    public class LootChests
    {
        [JsonProperty(PropertyName = "LootChest")]
        public IEnumerable<LootChest> LootChest { get; set; }
    }

    public class LootChest
    {
        [JsonProperty(PropertyName = "@uniquename")]
        public string UniqueName { get; set; }

        [JsonProperty(PropertyName = "@faction")]
        public string Faction { get; set; }
    }
}