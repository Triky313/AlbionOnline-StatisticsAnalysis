using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class Loot
    {
        public Loot()
        {
            UtcPickupTime = DateTime.UtcNow;
        }

        public int ItemIndex { get; set; }
        public DateTime UtcPickupTime { get; }
        public int Quantity { get; set; }
        public string LootedBody { get; set; }
        public string LooterName { get; set; }
        public bool IsSilver { get; set; }
        public bool IsTrash { get; set; }
    }
}