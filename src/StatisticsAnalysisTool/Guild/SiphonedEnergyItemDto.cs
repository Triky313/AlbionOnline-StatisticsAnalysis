using System;

namespace StatisticsAnalysisTool.Guild;

public class SiphonedEnergyItemDto
{
    public string GuildName { get; set; }
    public string CharacterName { get; set; }
    public long QuantityInternal { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsDisabled { get; set; }
}