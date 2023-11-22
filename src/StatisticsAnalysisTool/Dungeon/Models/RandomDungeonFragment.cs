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
    private int _level = -1;
    private double _might;
    private double _favor;
    private double _factionCoins;
    private double _factionFlags;
    private CityFaction _cityFaction = CityFaction.Unknown;
    private int _numberOfFloors;
    private Visibility _mightFavorVisibility = Visibility.Collapsed;
    private Visibility _factionPointsVisibility = Visibility.Collapsed;
    private double _mightPerHour;
    private double _favorPerHour;
    private double _factionCoinsPerHour;
    private double _factionFlagsPerHour;
    private string _levelString = "?";

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
        FactionFlags = dto.FactionFlags;
        CityFaction = dto.CityFaction;
        UpdateNumberOfFloors(null, null);
        UpdateValueVisibility();

        GuidList.CollectionChanged += UpdateNumberOfFloors;
    }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            LevelString = SetLevelString(_level);
            OnPropertyChanged();
        }
    }

    public double Might
    {
        get => _might;
        set
        {
            _might = value;
            MightPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double Favor
    {
        get => _favor;
        set
        {
            _favor = value;
            FavorPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double FactionCoins
    {
        get => _factionCoins;
        set
        {
            _factionCoins = value;
            FactionCoinsPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateValueVisibility();
            OnPropertyChanged();
        }
    }

    public double FactionFlags
    {
        get => _factionFlags;
        set
        {
            _factionFlags = value;
            FactionFlagsPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            OnPropertyChanged();
        }
    }

    public CityFaction CityFaction
    {
        get => _cityFaction;
        set
        {
            _cityFaction = value;
            OnPropertyChanged();
        }
    }

    #region Composite values that are not in the DTO

    public int NumberOfFloors
    {
        get => _numberOfFloors;
        set
        {
            _numberOfFloors = value;
            OnPropertyChanged();
        }
    }

    public Visibility MightFavorVisibility
    {
        get => _mightFavorVisibility;
        set
        {
            _mightFavorVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility FactionPointsVisibility
    {
        get => _factionPointsVisibility;
        set
        {
            _factionPointsVisibility = value;
            OnPropertyChanged();
        }
    }

    public double MightPerHour
    {
        get => double.IsNaN(_mightPerHour) ? 0 : _mightPerHour;
        private set
        {
            _mightPerHour = value;
            OnPropertyChanged();
        }
    }

    public double FavorPerHour
    {
        get => double.IsNaN(_favorPerHour) ? 0 : _favorPerHour;
        private set
        {
            _favorPerHour = value;
            OnPropertyChanged();
        }
    }

    public double FactionCoinsPerHour
    {
        get => double.IsNaN(_factionCoinsPerHour) ? 0 : _factionCoinsPerHour;
        private set
        {
            _factionCoinsPerHour = value;
            OnPropertyChanged();
        }
    }

    public double FactionFlagsPerHour
    {
        get => double.IsNaN(_factionFlagsPerHour) ? 0 : _factionFlagsPerHour;
        private set
        {
            _factionFlagsPerHour = value;
            OnPropertyChanged();
        }
    }

    public string LevelString
    {
        get => _levelString;
        private set
        {
            _levelString = value;
            OnPropertyChanged();
        }
    }

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

    // Flat-Map: 16% (green), .1-Map 36% (blue), .2-Map 58% (purple), .3-Map 84% (gold)
    private static string SetLevelString(int level)
    {
        var levelString = level switch
        {
            0 => string.Empty,
            1 => ".0",
            2 => ".1",
            3 => ".2",
            4 => ".3",
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
}