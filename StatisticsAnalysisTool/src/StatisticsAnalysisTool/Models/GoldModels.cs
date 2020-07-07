namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;
    using System;

    public class GoldResponseModel
    {
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public DateTime Timestamp { get; set; }
    }
}