namespace StatisticsAnalysisTool.Models.NetworkModel;

public class DiedObject
{
    public DiedObject(string diedName, string killedBy, string killedByGuild)
    {
        DiedName = diedName;
        KilledBy = killedBy;
        KilledByGuild = killedByGuild;
    }

    public string DiedName { get; }

    public string KilledBy { get; }

    public string KilledByGuild { get; }
}