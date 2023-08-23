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
    private double _might;
    private double _favor;
    private double _factionCoins;
    private double _factionFlags;
    private CityFaction _cityFaction = CityFaction.Unknown;
    private int _numberOfFloors;
    private Visibility _isFactionWarfareVisible = Visibility.Collapsed;
    private Visibility _isMightFavorVisible = Visibility.Collapsed;
    private double _mightPerHour;
    private double _favorPerHour;

    public RandomDungeonFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex) : base(guid, mapType, mode, mainMapIndex)
    {
        NumberOfFloors = 1;
        GuidList.CollectionChanged += UpdateGuids;
    }

    public RandomDungeonFragment(DungeonDto dto) : base(dto)
    {
        Might = dto.Might;
        Favor = dto.Favor;
        FactionCoins = dto.FactionCoins;
        FactionFlags = dto.FactionFlags;
        CityFaction = dto.CityFaction;

        GuidList.CollectionChanged += UpdateGuids;
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

    public Visibility IsFactionWarfareVisible
    {
        get => _isFactionWarfareVisible;
        set
        {
            _isFactionWarfareVisible = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsMightFavorVisible
    {
        get => _isMightFavorVisible;
        set
        {
            _isMightFavorVisible = value;
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

    #endregion

    private void UpdateGuids(object sender, NotifyCollectionChangedEventArgs e)
    {
        NumberOfFloors = GuidList.Count;
    }

    private void UpdateValueVisibility()
    {
        if (Favor is > 0 or > 0 && IsMightFavorVisible != Visibility.Visible)
        {
            IsMightFavorVisible = Visibility.Visible;
            IsFactionWarfareVisible = Visibility.Collapsed;
        }
        else if (FactionCoins > 0 && IsFactionWarfareVisible == Visibility.Collapsed)
        {
            IsMightFavorVisible = Visibility.Collapsed;
            IsFactionWarfareVisible = Visibility.Visible;
        }
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