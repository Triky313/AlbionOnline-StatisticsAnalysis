namespace StatisticsAnalysisTool.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Player
    {
        public double AverageItemPower { get; set; }
        public Equipment Equipment { get; set; }
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
        public LifetimeStatistics LifetimeStatistics { get; set; }
    }

    public class Equipment
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

        [JsonProperty(PropertyName = "Cape")]
        public object Mount { get; set; }

        [JsonProperty(PropertyName = "Cape")]
        public object Potion { get; set; }

        [JsonProperty(PropertyName = "Cape")]
        public object Food { get; set; }
    }
    
    public class PvE
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
        public int Hellgate { get; set; }
    }

    public class Fiber
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class Hide
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class Ore
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class Rock
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class Wood
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class All
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class Gathering
    {
        public Fiber Fiber { get; set; }
        public Hide Hide { get; set; }
        public Ore Ore { get; set; }
        public Rock Rock { get; set; }
        public Wood Wood { get; set; }
        public All All { get; set; }
    }

    public class Crafting
    {
        public int Total { get; set; }
        public int Royal { get; set; }
        public int Outlands { get; set; }
    }

    public class LifetimeStatistics
    {
        public PvE PvE { get; set; }
        public Gathering Gathering { get; set; }
        public Crafting Crafting { get; set; }
        public int CrystalLeague { get; set; }
        public DateTime Timestamp { get; set; }
    }

}