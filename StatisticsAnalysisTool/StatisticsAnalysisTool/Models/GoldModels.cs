using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class GoldResponseModel
    {
        [JsonPropertyName("price")]
        public int Price { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}