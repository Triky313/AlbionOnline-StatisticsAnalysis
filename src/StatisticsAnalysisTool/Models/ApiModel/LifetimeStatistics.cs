namespace StatisticsAnalysisTool.Models.ApiModel;

public class LifetimeStatistics
{
    public PvE PvE { get; set; }
    public Gathering Gathering { get; set; }
    public Crafting Crafting { get; set; }
    public int CrystalLeague { get; set; }
    public int FishingFame { get; set; }
    public int FarmingFame { get; set; }
    public object Timestamp { get; set; }
}