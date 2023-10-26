using System.Collections.Generic;

namespace StatisticsAnalysisTool.Guild;

public class GuildDto
{
    public List<SiphonedEnergyItemDto> SiphonedEnergies { get; set; } = new ();
}