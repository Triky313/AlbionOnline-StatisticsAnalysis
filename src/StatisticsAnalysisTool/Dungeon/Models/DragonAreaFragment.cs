using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using System;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DragonAreaFragment : DungeonBaseFragment
{
    public DragonAreaFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex) : base(guid, mapType, mode, mainMapIndex)
    {
        Faction = Faction.DragonArea;
    }

    public DragonAreaFragment(DungeonDto dto) : base(dto)
    {
        Faction = Faction.DragonArea;
        Might = dto.Might;
        Favor = dto.Favor;
        PortalSize = ResolvePortalSize(dto);

        UpdateValueVisibility();
    }
    public int NumberOfFloors => GuidList.Count;


    public DragonAreaPortalSize PortalSize
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }
    public double Might
    {
        get;
        set
        {
            field = value;
            MightPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double Favor
    {
        get;
        set
        {
            field = value;
            FavorPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double MightPerHour
    {
        get => double.IsNaN(field) ? 0 : field;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FavorPerHour
    {
        get => double.IsNaN(field) ? 0 : field;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility MightFavorVisibility
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

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
            case ValueType.Might:
                Might += value;
                return;
            case ValueType.Favor:
                Favor += value;
                return;
        }
    }

    private void UpdateValueVisibility()
    {
        if ((Favor > 0 || Might > 0) && MightFavorVisibility != Visibility.Visible)
        {
            MightFavorVisibility = Visibility.Visible;
        }
    }

    private static DragonAreaPortalSize ResolvePortalSize(DungeonDto dto)
    {
        if (dto.DragonAreaPortalSize != DragonAreaPortalSize.Unknown)
        {
            return dto.DragonAreaPortalSize;
        }

        foreach (var dungeonEvent in dto.Events ?? [])
        {
            var portalSize = DragonAreaPortalSizeResolver.FromUniqueName(dungeonEvent.UniqueName);
            if (portalSize != DragonAreaPortalSize.Unknown)
            {
                return portalSize;
            }
        }

        return DragonAreaPortalSize.Unknown;
    }
}