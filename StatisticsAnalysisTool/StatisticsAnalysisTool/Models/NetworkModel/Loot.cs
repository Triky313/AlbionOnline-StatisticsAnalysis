using Newtonsoft.Json;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class Loot
    {
        [JsonIgnore]
        public int ObjectId { get; set; }

        [JsonIgnore]
        public int ItemRefId { get; set; }

        [JsonProperty]
        public DateTime LocalPickupTime { get; set; }

        [JsonProperty]
        public DateTime UtcPickupTime { get; set; }

        [JsonProperty]
        public string BodyName { get; set; }

        [JsonProperty]
        public string ItemName { get; set; }
        [JsonProperty]
        public string LongName { get; set; }
        [JsonProperty]
        public int Quantity { get; set; }

        [JsonIgnore]
        public string LooterName { get; set; }

        public string GetLine()
        {
            return string.Join(", ", LocalPickupTime.ToString("HH:mm:ss"), LooterName, Quantity, ItemName, BodyName) + "\\n";
        }

        public override string ToString()
        {
            return string.Join(", ", LocalPickupTime.ToString("HH:mm:ss"), LooterName, Quantity, LongName, BodyName);
        }
    }
}
