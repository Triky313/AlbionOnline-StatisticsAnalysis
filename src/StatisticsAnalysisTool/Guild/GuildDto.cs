using System.Collections.Generic;

namespace StatisticsAnalysisTool.Guild;

public class GuildDto
{
    public string GuildId { get; set; }
    public List<SiphonedEnergyItem> SiphonedEnergy { get; set; }
}