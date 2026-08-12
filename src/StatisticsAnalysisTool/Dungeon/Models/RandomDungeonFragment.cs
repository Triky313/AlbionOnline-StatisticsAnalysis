using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Specialized;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class RandomDungeonFragment : DungeonBaseFragment
{
    public bool IsLevelLockedFromEntrance { get; private set; }
    public bool IsTierLockedFromEntrance { get; private set; }
    public double MobHitPointsFactor { get; set; } = 1;
    public double ZoneLootFactor { get; set; }

    public RandomDungeonFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex) : base(guid, mapType, mode, mainMapIndex)
    {
        NumberOfFloors = 1;
        GuidList.CollectionChanged += UpdateNumberOfFloors;
    }

    public RandomDungeonFragment(DungeonDto dto) : base(dto)
    {
        Level = dto.Level;
        Might = dto.Might;
        Favor = dto.Favor;
        FactionCoins = dto.FactionCoins;
        FactionStanding = dto.FactionStanding;
        CityFaction = dto.CityFaction;
        UpdateNumberOfFloors(null, null);
        UpdateValueVisibility();

        GuidList.CollectionChanged += UpdateNumberOfFloors;
    }

    public int Level
    {
        get;
        private set
        {
            field = value;
            LevelString = SetLevelString(field);
            OnPropertyChanged();
        }
    } = -1;

    public double Might
    {
        get;
        set
        {
            field = value;
            MightPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0
                ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds
                : TotalRunTimeInSeconds);
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
            FavorPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0
                ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds
                : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double FactionCoins
    {
        get;
        set
        {
            field = value;
            FactionCoinsPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0
                ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds
                : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double FactionStanding
    {
        get;
        set
        {
            field = value;
            FactionStandingPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0
                ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds
                : TotalRunTimeInSeconds);
            OnPropertyChanged();
        }
    }

    public CityFaction CityFaction
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = CityFaction.Unknown;

    #region Composite values that are not in the DTO

    public int NumberOfFloors
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility MightFavorVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility FactionPointsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

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

    public double FactionCoinsPerHour
    {
        get => double.IsNaN(field) ? 0 : field;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FactionStandingPerHour
    {
        get => double.IsNaN(field) ? 0 : field;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string LevelString
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "?";

    #endregion

    private void UpdateNumberOfFloors(object sender, NotifyCollectionChangedEventArgs e)
    {
        NumberOfFloors = GuidList.Count;
    }

    private void UpdateValueVisibility()
    {
        if ((Favor > 0 || Might > 0) && MightFavorVisibility != Visibility.Visible)
        {
            MightFavorVisibility = Visibility.Visible;
            FactionPointsVisibility = Visibility.Collapsed;
        }
        else if (FactionCoins > 0 && FactionPointsVisibility != Visibility.Visible)
        {
            MightFavorVisibility = Visibility.Collapsed;
            FactionPointsVisibility = Visibility.Visible;
        }
    }

    private static string SetLevelString(int level)
    {
        var levelString = level switch
        {
            0 => ".0",
            1 => ".1",
            2 => ".2",
            3 => ".3",
            4 => ".4",
            _ => ".?"
        };

        return levelString;
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
            case ValueType.FactionStanding:
                if (cityFaction != CityFaction.Unknown)
                {
                    FactionStanding += value;
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

    public bool TrySetLevelFromEntrance(int level)
    {
        if (level is < 0 or > 4)
        {
            return false;
        }

        IsLevelLockedFromEntrance = true;
        if (Level == level)
        {
            return false;
        }

        Level = level;
        return true;
    }

    public bool TrySetLevelFromMob(int level)
    {
        if (IsLevelLockedFromEntrance || level is < 0 or > 4 || level <= Level)
        {
            return false;
        }

        Level = level;
        return true;
    }

    public bool TrySetTierFromEntrance(Tier tier)
    {
        if (tier == Tier.Unknown)
        {
            return false;
        }

        IsTierLockedFromEntrance = true;
        if (Tier == tier)
        {
            return false;
        }

        Tier = tier;
        return true;
    }

    public bool TrySetTierFromMob(Tier tier)
    {
        if (IsTierLockedFromEntrance || tier == Tier.Unknown || Tier == tier)
        {
            return false;
        }

        Tier = tier;
        return true;
    }

    public bool TrySetLevelFromLootFactor(int level)
    {
        if (IsLevelLockedFromEntrance || level is < 0 or > 4)
        {
            return false;
        }

        if (Level == level)
        {
            return false;
        }

        Level = level;
        return true;
    }
}