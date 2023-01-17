using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public class EquipmentItemInternal
{
    public EquipmentItemInternal()
    {
        TimeStamp = DateTime.UtcNow;
    }

    public DateTime TimeStamp { get; }
    public int ItemIndex { get; set; }
    public Dictionary<int, int> SpellDictionary { get; set; }
}