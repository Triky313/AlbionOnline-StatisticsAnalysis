using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ApiModel;

public class GameInfoPlayerKillsDeathsWithType
{
    public GameInfoPlayerKillsDeathsType ObjectType { get; set; }
    public int GroupMemberCount { get; set; }
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

    protected bool Equals(GameInfoPlayerKillsDeaths other)
    {
        return EventId == other.EventId && TimeStamp.Equals(other.TimeStamp);
    }
}