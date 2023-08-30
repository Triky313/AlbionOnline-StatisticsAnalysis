using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class MistsFragment : DungeonBaseFragment
{
    private double _might;
    private double _favor;
    private double _brecilianStanding;
    private double _mightPerHour;
    private double _favorPerHour;
    private double _brecilianStandingPerHour;
    private MistsRarity _rarity;
    private Visibility _mightFavorVisibility = Visibility.Collapsed;
    private Visibility _brecilianStandingVisibility = Visibility.Collapsed;

    public MistsFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex, MistsRarity rarity, Tier tier) : base(guid, mapType, mode, mainMapIndex)
    {
        Rarity = rarity;
        Tier = tier;
    }

    public MistsFragment(DungeonDto dto) : base(dto)
    {
        Might = dto.Might;
        Favor = dto.Favor;
        Rarity = dto.MistsRarity;
        BrecilianStanding = dto.BrecilianStanding;

        UpdateValueVisibility();
        UpdateBrecilianStandingVisibility();
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
            OnPropertyChanged();
        }
    }

    public double BrecilianStanding
    {
        get => _brecilianStanding;
        set
        {
            _brecilianStanding = value;
            BrecilianStandingPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateBrecilianStandingVisibility();
            OnPropertyChanged();
        }
    }

    public MistsRarity Rarity
    {
        get => _rarity;
        set
        {
            _rarity = value;
            OnPropertyChanged();
        }
    }

    #region Composite values that are not in the DTO

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

    public double BrecilianStandingPerHour
    {
        get => double.IsNaN(_brecilianStandingPerHour) ? 0 : _brecilianStandingPerHour;
        private set
        {
            _brecilianStandingPerHour = value;
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

    public Visibility BrecilianStandingVisibility
    {
        get => _brecilianStandingVisibility;
        set
        {
            _brecilianStandingVisibility = value;
            OnPropertyChanged();
        }
    }

    #endregion

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
            case ValueType.BrecilianStanding:
                BrecilianStanding += value;
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

    private void UpdateBrecilianStandingVisibility()
    {
        if (BrecilianStanding > 0 && BrecilianStandingVisibility != Visibility.Visible)
        {
            BrecilianStandingVisibility = Visibility.Visible;
        }
    }
}