using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class Loot
    {
        public Loot()
        {
            UtcPickupTime = DateTime.UtcNow;
        }

        public int ItemId { get; set; }
        [JsonIgnore] 
        public Item Item { get; set; }
        public DateTime UtcPickupTime { get; }
        public int Quantity { get; set; }
        public string LootedBody { get; set; }
        public string LooterName { get; set; }
        public bool IsSilver { get; set; }
        public bool IsTrash { get; set; }
    }
}