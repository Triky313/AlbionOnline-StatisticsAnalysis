namespace StatisticsAnalysisTool.Models
{
    using System;
    using System.Collections.Generic;

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

    public class EquipmentResponse
    {
        public object MainHand { get; set; }
        public object OffHand { get; set; }
        public object Head { get; set; }
        public object Armor { get; set; }
        public object Shoes { get; set; }
        public object Bag { get; set; }
        public object Cape { get; set; }
        public object Mount { get; set; }
        public object Potion { get; set; }
        public object Food { get; set; }
    }
    
    public class PvEResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
        public ulong Hellgate { get; set; }
    }

    public class FiberResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class HideResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class OreResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class RockResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class WoodResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class AllResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class GatheringResponse
    {
        public FiberResponse Fiber { get; set; }
        public HideResponse Hide { get; set; }
        public OreResponse Ore { get; set; }
        public RockResponse Rock { get; set; }
        public WoodResponse Wood { get; set; }
        public AllResponse All { get; set; }
    }

    public class CraftingResponse
    {
        public ulong Total { get; set; }
        public ulong Royal { get; set; }
        public ulong Outlands { get; set; }
    }

    public class LifetimeStatisticsResponse
    {
        public PvEResponse PvE { get; set; }
        public GatheringResponse Gathering { get; set; }
        public CraftingResponse Crafting { get; set; }
        public int CrystalLeague { get; set; }
        public DateTime Timestamp { get; set; }
    }

}