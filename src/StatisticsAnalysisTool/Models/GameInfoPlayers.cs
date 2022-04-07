using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models
{
    public class GameInfoPlayersResponse
    {
        public double AverageItemPower { get; set; }
        public EquipmentResponse EquipmentResponse { get; set; }
        public List<object> Inventory { get; set; }
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
        public long KillFame { get; set; }
        public double FameRatio { get; set; }
        public LifetimeStatisticsResponse LifetimeStatistics { get; set; }
    }
}