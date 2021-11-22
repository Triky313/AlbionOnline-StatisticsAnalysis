using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models
{
    public class EquipmentItem
    {
        public EquipmentItem()
        {
            TimeStamp = DateTime.UtcNow;
        }

        public DateTime TimeStamp { get; }
        public int ItemIndex { get; set; }
        public Dictionary<int, int> SpellDictionary { get; set; }
    }
}