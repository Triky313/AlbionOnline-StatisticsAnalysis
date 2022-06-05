using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class GroupMember
{
    public double AverageItemPower { get; set; }
    [JsonIgnore]
    public Equipment Equipment { get; set; }
    [JsonIgnore]
    public List<object> Inventory { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public string GuildName { get; set; }
    public string GuildId { get; set; }
    public string AllianceName { get; set; }
    public string AllianceId { get; set; }
    public string AllianceTag { get; set; }
    [JsonIgnore]
    public string Avatar { get; set; }
    [JsonIgnore]
    public string AvatarRing { get; set; }
    public int DeathFame { get; set; }
    public int KillFame { get; set; }
    public double FameRatio { get; set; }
    [JsonIgnore]
    public LifetimeStatistics LifetimeStatistics { get; set; }
}