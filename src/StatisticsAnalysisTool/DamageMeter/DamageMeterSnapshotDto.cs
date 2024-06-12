using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshotDto
{
    public DateTime Timestamp { get; set; }
    public List<DamageMeterSnapshotFragmentDto> DamageMeter { get; set; }
}