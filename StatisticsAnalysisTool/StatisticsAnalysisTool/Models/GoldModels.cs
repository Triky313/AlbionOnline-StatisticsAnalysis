using System;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class GoldResponseModel
    {
        [JsonProperty(PropertyName = "price")] public int Price { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}