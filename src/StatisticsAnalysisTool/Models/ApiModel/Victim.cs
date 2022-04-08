using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class Victim
{
    public double AverageItemPower { get; set; }
    //public Equipment Equipment { get; set; }
    //public List<Inventory> Inventory { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public string GuildName { get; set; }
    public string GuildId { get; set; }
    public string AllianceName { get; set; }
    public string AllianceId { get; set; }
    public string AllianceTag { get; set; }
    public string Avatar { get; set; }
    public string AvatarRing { get; set; }
    public int DeathFame { get; set; }
    public int KillFame { get; set; }
    public double FameRatio { get; set; }
    //public LifetimeStatistics LifetimeStatistics { get; set; }
}