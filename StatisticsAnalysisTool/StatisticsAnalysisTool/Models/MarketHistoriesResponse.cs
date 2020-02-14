namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;

    public class MarketHistoriesResponse
    {
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "itemTypeId")]
        public string ItemTypeId { get; set; }

        [JsonProperty(PropertyName = "qualityLevel")]
        public int QualityLevel { get; set; }

        [JsonProperty(PropertyName = "data")]
        public MarketHistoryResponse Data { get; set; }
    }
}