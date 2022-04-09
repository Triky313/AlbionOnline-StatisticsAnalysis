using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class OffHand
{
    public string Type { get; set; }
    public int Count { get; set; }
    public int Quality { get; set; }
    public List<object> ActiveSpells { get; set; }
    public List<object> PassiveSpells { get; set; }
}