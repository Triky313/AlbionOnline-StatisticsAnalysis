using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DungeonDto
{
    public DungeonMode Mode { get; set; }
    public MapType MapType { get; set; }
    public string MainMapIndex { get; set; }
    public List<Guid> GuidList { get; set; } = new();
    public Tier Tier { get; set; } = Tier.Unknown;
    public int Level { get; set; } = -1;
    public Faction Faction { get; set; } = Faction.Unknown;
    public CityFaction CityFaction { get; set; } = CityFaction.Unknown;
    public DateTime EnterDungeonFirstTime { get; set; }
    public int TotalRunTimeInSeconds { get; set; }
    public int PartySize { get; set; } = 1;
    public List<DungeonEventDto> Events { get; set; } = new();
    public List<LootDto> Loot { get; set; } = new();
    public DungeonStatus Status { get; set; }
    public double Fame { get; set; }
    public double ReSpec { get; set; }
    public double Silver { get; set; }
    public double Might { get; set; }
    public double Favor { get; set; }
    public double BrecilianStanding { get; set; }
    public double FactionCoins { get; set; }
    public double FactionStanding { get; set; }

    [JsonPropertyName("FactionFlags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double LegacyFactionFlags
    {
        get => 0;
        set
        {
            if (FactionStanding == 0)
            {
                FactionStanding = value;
            }
        }
    }

    public string DiedName { get; set; }
    public string KilledBy { get; set; }
    public KillStatus KillStatus { get; set; }
    public MistsRarity MistsRarity { get; set; }
    public List<CheckPointDto> CheckPoints { get; set; }
}