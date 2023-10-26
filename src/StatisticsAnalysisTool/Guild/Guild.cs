using System.Collections.Generic;

namespace StatisticsAnalysisTool.Guild;

public class Guild
{
    public string GuildId { get; set; }
    public List<SiphonedEnergyItem> SiphonedEnergies { get; set; } = new ();
}