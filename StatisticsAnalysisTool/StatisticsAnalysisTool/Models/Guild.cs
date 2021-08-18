using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class GameInfoGuildsResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FounderId { get; set; }
        public string FounderName { get; set; }
        public DateTime Founded { get; set; }
        public string AllianceTag { get; set; }
        public string AllianceId { get; set; }
        public object AllianceName { get; set; }
        public object Logo { get; set; }

        [JsonPropertyName("killFame")]
        public ulong KillFame { get; set; }

        public ulong DeathFame { get; set; }
        public object AttacksWon { get; set; }
        public object DefensesWon { get; set; }
        public int MemberCount { get; set; }
    }
}