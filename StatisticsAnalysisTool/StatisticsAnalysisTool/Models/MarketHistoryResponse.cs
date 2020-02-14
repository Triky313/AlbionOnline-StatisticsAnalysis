namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;
    using System;

    public class MarketHistoryResponse
    {
        [JsonProperty(PropertyName = "itemCount")]
        public int ItemCount { get; set; }

        [JsonProperty(PropertyName = "averagePrice")]
        public int AveragePrice { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}