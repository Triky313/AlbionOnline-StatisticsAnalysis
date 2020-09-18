using Newtonsoft.Json;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models
{
    public class MarketStatResponse
    {
        [JsonProperty(PropertyName = "timestamps")]
        public List<ulong> TimeStamps { get; set; }

        [JsonProperty(PropertyName = "prices_min")]
        public List<ulong> PricesMin { get; set; }

        [JsonProperty(PropertyName = "prices_max")]
        public List<ulong> PricesMax { get; set; }

        [JsonProperty(PropertyName = "prices_avg")]
        public List<decimal> PricesAvg { get; set; }
    }

    public class MarketStatChartResponse
    {
        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "data")] public MarketStatResponse Data { get; set; }
    }
}