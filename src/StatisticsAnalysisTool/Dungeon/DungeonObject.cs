using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Dungeon.Models;
using Loot = StatisticsAnalysisTool.Dungeon.Models.Loot;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon;

[Obsolete]
public class DungeonObject
{
    [JsonIgnore]
    public List<TimeCollectObject> DungeonRunTimes { get; } = new();
    public int TotalRunTimeInSeconds { get; set; }
    public List<Guid> GuidList { get; set; } = new();
    public DateTime EnterDungeonFirstTime { get; set; }
    public string MainMapIndex { get; set; }
    public List<PointOfInterest> DungeonEventObjects { get; set; } = new();
    public List<Loot> DungeonLoot { get; set; } = new();
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
    [JsonIgnore]
    public string DungeonHash => $"{EnterDungeonFirstTime.Ticks}{string.Join(",", GuidList)}";
    [JsonIgnore]
    public Loot MostExpensiveLoot => DungeonLoot.MaxBy(x => x.EstimatedMarketValueInternal);
    [JsonIgnore]
    public long TotalLootInSilver => DungeonLoot.Sum(x => x.EstimatedMarketValue.IntegerValue);

    public DungeonObject()
    {
    }

    public DungeonObject(string mainMapIndex, Guid guid, DungeonStatus status)
    {
        MainMapIndex = mainMapIndex;
        EnterDungeonFirstTime = DateTime.UtcNow;
        GuidList.Add(guid);
        Status = status;
        AddTimer(DateTime.UtcNow);
        Mode = Mode == DungeonMode.Unknown ? DungeonData.GetDungeonMode(mainMapIndex) : Mode;
    }

    public void Add(double value, ValueType type, CityFaction cityFaction = CityFaction.Unknown)
    {
        switch (type)
        {
            case ValueType.Fame:
                Fame += value;
                return;
            case ValueType.ReSpec:
                ReSpec += value;
                return;
            case ValueType.Silver:
                Silver += value;
                return;
            case ValueType.FactionFame:
                if (cityFaction != CityFaction.Unknown)
                {
                    FactionFlags += value;
                }
                return;
            case ValueType.FactionPoints:
                if (cityFaction != CityFaction.Unknown)
                {
                    FactionCoins += value;
                    CityFaction = cityFaction;
                }
                return;
            case ValueType.Might:
                Might += value;
                return;
            case ValueType.Favor:
                Favor += value;
                return;
        }
    }

    public void AddTimer(DateTime time)
    {
        if (DungeonRunTimes.Any(x => x.EndTime == null))
        {
            var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
            if (dun != null)
            {
                dun.EndTime = time;
                DungeonRunTimes.Add(new TimeCollectObject(time));
            }
        }
        else
        {
            DungeonRunTimes.Add(new TimeCollectObject(time));
        }

        SetTotalRunTimeInSeconds();
    }

    public void EndTimer()
    {
        var dateTime = DateTime.UtcNow;

        var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
        if (dun != null && dun.StartTime < dateTime)
        {
            dun.EndTime = dateTime;
            SetTotalRunTimeInSeconds();
        }
    }

    public void SetTier(Tier tier)
    {
        if (Tier != Tier.Unknown)
        {
            return;
        }

        Tier = tier;
    }

    public void SetLevel(int level)
    {
        if (Level >= 0)
        {
            return;
        }

        Level = level;
    }

    private void SetTotalRunTimeInSeconds()
    {
        foreach (var time in DungeonRunTimes.Where(x => x.EndTime != null).ToList())
        {
            TotalRunTimeInSeconds += (int) time.TimeSpan.TotalSeconds;
            DungeonRunTimes.Remove(time);
        }
    }
}