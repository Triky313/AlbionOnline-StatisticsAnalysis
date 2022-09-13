using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DiscoveredItem
    {
        public DiscoveredItem()
        {
            UtcDiscoveryTime = DateTime.UtcNow;
        }

        public long ObjectId { get; set; }
        public int ItemIndex { get; set; }
        public DateTime UtcDiscoveryTime { get; }
        public int Quantity { get; set; }
        public string BodyName { get; set; }
        public string LooterName { get; set; }
        public FixPoint EstimatedMarketValue { get; set; }
        public Dictionary<int, int> SpellDictionary { get; set; }
    }
}