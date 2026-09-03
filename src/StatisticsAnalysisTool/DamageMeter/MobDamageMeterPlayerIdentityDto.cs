using System;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterPlayerIdentityDto
{
    public Guid CauserGuid { get; set; }
    public string Name { get; set; } = string.Empty;
}