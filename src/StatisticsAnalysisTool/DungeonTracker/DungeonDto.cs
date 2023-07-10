using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DungeonTracker;

public class DungeonDto
{
    public int TotalRunTimeInSeconds { get; set; }
    public List<Guid> GuidList { get; set; } = new();
    public DateTime EnterDungeonFirstTime { get; set; }
    public string MainMapIndex { get; set; }
    public List<DungeonEventObject> DungeonEventObjects { get; set; } = new();
    public List<DungeonLoot> DungeonLoot { get; set; } = new();
    public DungeonStatus Status { get; set; }
    public double Fame { get; set; }
    public double ReSpec { get; set; }
    public double Silver { get; set; }
    public double Might { get; set; }
    public double Favor { get; set; }
    public double FactionCoins { get; set; }
    public double FactionFlags { get; set; }
    public string DiedName { get; set; }
    public string KilledBy { get; set; }
    public bool DiedInDungeon { get; set; }
    public Faction Faction { get; set; } = Faction.Unknown;
    public DungeonMode Mode { get; set; } = DungeonMode.Unknown;
    public CityFaction CityFaction { get; set; } = CityFaction.Unknown;
    public Tier Tier { get; set; } = Tier.Unknown;
    public int Level { get; set; } = -1;
}