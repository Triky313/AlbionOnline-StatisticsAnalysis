using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class MarketStatResponse
    {
        [JsonPropertyName("timestamps")]
        public List<ulong> TimeStamps { get; set; }

        [JsonPropertyName("prices_min")]
        public List<ulong> PricesMin { get; set; }

        [JsonPropertyName("prices_max")]
        public List<ulong> PricesMax { get; set; }

        [JsonPropertyName("prices_avg")]
        public List<decimal> PricesAvg { get; set; }
    }

    public class MarketStatChartResponse
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("data")] public MarketStatResponse Data { get; set; }
    }
}