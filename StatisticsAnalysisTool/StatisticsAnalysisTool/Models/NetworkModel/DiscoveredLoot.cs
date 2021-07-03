using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DiscoveredLoot
    {
        public DiscoveredLoot()
        {
            UtcPickupTime = DateTime.UtcNow;
        }

        public long ObjectId { get; set; }
        public int ItemId { get; set; }
        public DateTime UtcPickupTime { get; }
        public int Quantity { get; set; }
        public string BodyName { get; set; }
        public string LooterName { get; set; }
        public Dictionary<int, int> SpellDictionary { get; set; }
    }
}