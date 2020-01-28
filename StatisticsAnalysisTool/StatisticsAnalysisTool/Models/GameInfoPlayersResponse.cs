namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;
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
        public LifetimeStatisticsResponse LifetimeStatisticsResponse { get; set; }
    }

    public class EquipmentResponse
    {
        [JsonProperty(PropertyName = "MainHand")]
        public object MainHand { get; set; }

        [JsonProperty(PropertyName = "OffHand")]
        public object OffHand { get; set; }

        [JsonProperty(PropertyName = "Head")]
        public object Head { get; set; }

        [JsonProperty(PropertyName = "Armor")]
        public object Armor { get; set; }

        [JsonProperty(PropertyName = "Shoes")]
        public object Shoes { get; set; }

        [JsonProperty(PropertyName = "Bag")]
        public object Bag { get; set; }

        [JsonProperty(PropertyName = "Cape")]
        public object Cape { get; set; }

        [JsonProperty(PropertyName = "Mount")]
        public object Mount { get; set; }

        [JsonProperty(PropertyName = "Potion")]
        public object Potion { get; set; }

        [JsonProperty(PropertyName = "Food")]
        public object Food { get; set; }
    }
    
    public class PvEResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
        public int Hellgate { get; set; }
    }

    public class FiberResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class HideResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class OreResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class RockResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class WoodResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class AllResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class GatheringResponse
    {
        public FiberResponse FiberResponse { get; set; }
        public HideResponse HideResponse { get; set; }
        public OreResponse OreResponse { get; set; }
        public RockResponse RockResponse { get; set; }
        public WoodResponse WoodResponse { get; set; }
        public AllResponse AllResponse { get; set; }
    }

    public class CraftingResponse
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class LifetimeStatisticsResponse
    {
        public PvEResponse PvEResponse { get; set; }
        public GatheringResponse GatheringResponse { get; set; }
        public CraftingResponse CraftingResponse { get; set; }
        public int CrystalLeague { get; set; }
        public DateTime Timestamp { get; set; }
    }

}