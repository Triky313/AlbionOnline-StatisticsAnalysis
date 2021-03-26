using System;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class MarketHistoryResponse
    {
        [JsonProperty(PropertyName = "item_count")]
        public int ItemCount { get; set; }

        [JsonProperty(PropertyName = "avg_price")]
        public int AveragePrice { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}