using StatisticsAnalysisTool.Annotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class MarketHistoriesResponse
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("item_id")]
        public string ItemId { get; set; }

        [JsonPropertyName("quality")]
        public int Quality { get; set; }

        [JsonPropertyName("data")]
        [CanBeNull]
        public List<MarketHistoryResponse> Data { get; set; }
    }
}