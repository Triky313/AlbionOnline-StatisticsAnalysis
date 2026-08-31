using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using System;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class StaticDungeonFragment : DungeonBaseFragment
{
    public StaticDungeonFragment(Guid guid, string mainMapIndex, Faction faction, Tier tier)
        : base(guid, MapType.StaticDungeon, DungeonMode.StaticDungeon, mainMapIndex)
    {
        Faction = faction;
        Tier = tier;
    }

    public StaticDungeonFragment(DungeonDto dto) : base(dto)
    {
    }

    public void Add(double value, ValueType type)
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
        }
    }
}