using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.BindingModel;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringStats
{
    public GatheringFilterType GatheringFilterType { get; set; }
    public ItemTier Tier { get; set; }
    public List<ResourceStats> ResourceStats { get; set; }
}