using StatisticsAnalysisTool.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class ExpeditionFragment : DungeonBaseFragment
{
    private List<CheckPoint> _checkPoints = new();

    public ExpeditionFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex) : base(guid, mapType, mode, mainMapIndex)
    {
    }

    public ExpeditionFragment(DungeonDto dto) : base(dto)
    {
        CheckPoints = dto.CheckPoints.Select(DungeonMapping.Mapping).ToList();
    }

    public List<CheckPoint> CheckPoints
    {
        get => _checkPoints;
        set
        {
            _checkPoints = value;
            OnPropertyChanged();
        }
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