using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class GameInfoPlayerKillsDeaths
{
    [JsonPropertyName("groupMemberCount")]
    public int GroupMemberCount { get; set; }
    [JsonPropertyName("numberOfParticipants")]
    public int NumberOfParticipants { get; set; }
    public int EventId { get; set; }
    public DateTime TimeStamp { get; set; }
    public int Version { get; set; }
    public Killer Killer { get; set; }
    public Victim Victim { get; set; }
    public int TotalVictimKillFame { get; set; }
    public object Location { get; set; }
    //public List<Participant> Participants { get; set; }
    //public List<GroupMember> GroupMembers { get; set; }
    public object GvGMatch { get; set; }
    public int BattleId { get; set; }
    public string KillArea { get; set; }
    public object Category { get; set; }
    public string Type { get; set; }
}