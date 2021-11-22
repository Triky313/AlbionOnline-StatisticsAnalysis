using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class MarketHistoryResponse
    {
        [JsonPropertyName("item_count")]
        public int ItemCount { get; set; }

        [JsonPropertyName("avg_price")]
        public int AveragePrice { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}