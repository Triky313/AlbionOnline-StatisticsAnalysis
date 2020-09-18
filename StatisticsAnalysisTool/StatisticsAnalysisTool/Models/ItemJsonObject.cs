using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class ItemJsonObject
    {
        [JsonProperty("LocalizationNameVariable")]
        public string LocalizationNameVariable { get; set; }

        [JsonProperty("LocalizationDescriptionVariable")]
        public string LocalizationDescriptionVariable { get; set; }

        [JsonProperty("LocalizedNames")]
        public LocalizedNames LocalizedNames { get; set; }

        [JsonProperty("Index")]
        public int Index { get; set; }

        [JsonProperty("UniqueName")]
        public string UniqueName { get; set; }
    }
}