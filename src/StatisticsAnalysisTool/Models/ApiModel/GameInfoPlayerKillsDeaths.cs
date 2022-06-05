using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class GameInfoPlayerKillsDeaths
{
    [JsonPropertyName("groupMemberCount")]
    public int GroupMemberCount { get; init; }
    [JsonPropertyName("numberOfParticipants")]
    public int NumberOfParticipants { get; init; }
    public int EventId { get; init; }
    public DateTime TimeStamp { get; init; }
    public int Version { get; init; }
    public Killer Killer { get; init; }
    public Victim Victim { get; init; }
    public int TotalVictimKillFame { get; init; }
    public object Location { get; init; }
    //public List<Participant> Participants { get; set; }
    //public List<GroupMember> GroupMembers { get; set; }
    public object GvGMatch { get; init; }
    public int BattleId { get; init; }
    public string KillArea { get; init; }
    public object Category { get; init; }
    public string Type { get; init; }
}