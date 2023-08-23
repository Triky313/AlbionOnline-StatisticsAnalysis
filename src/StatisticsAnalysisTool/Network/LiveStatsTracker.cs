using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network;

public class LiveStatsTracker
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    private readonly List<ValuePerHour> _famePerHourList = new();
    private readonly List<ValuePerHour> _reSpecPerHourList = new();
    private readonly List<ValuePerHour> _silverPerHourList = new();
    private readonly List<ValuePerHour> _mightPerHourList = new();
    private readonly List<ValuePerHour> _favorPerHourList = new();
    private readonly List<ValuePerHour> _paidSilverForReSpecPerHourList = new();
    private readonly List<ValuePerHour> _factionPointsPerHourList = new();

    private double _famePerHourValue;
    private double _reSpecPerHourValue;
    private double _silverPerHourValue;
    private double _mightPerHourValue;
    private double _favorPerHourValue;
    private double _paidSilverForReSpecPerHourValue;
    private double _factionPointsPerHourValue;

    private DateTime _startTime;
    private double _totalGainedFameInSession;
    private double _totalGainedReSpecInSession;
    private double _totalGainedSilverInSession;
    private double _totalGainedMightInSession;
    private double _totalGainedFavorInSession;
    private double _totalPaidSilverForReSpecInSession;
    private double _totalGainedFactionPointsInSession;
    private CityFaction _currentCityFaction = CityFaction.Unknown;
    private readonly TrackingController _trackingController;
    private readonly DispatcherTimer _dispatcherTimer = new();

    public LiveStatsTracker(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void Add(ValueType valueType, double value, CityFaction cityFaction = CityFaction.Unknown)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        switch (valueType)
        {
            case ValueType.Fame:
                _famePerHourValue += value;
                _famePerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalGainedFameInSession += value;

                RemoveValueFromValuePerHour(_famePerHourList, ref _famePerHourValue);
                break;
            case ValueType.ReSpec:
                _reSpecPerHourValue += value;
                _reSpecPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalGainedReSpecInSession += value;

                RemoveValueFromValuePerHour(_reSpecPerHourList, ref _reSpecPerHourValue);
                break;
            case ValueType.Silver:
                _silverPerHourValue += value;
                _silverPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalGainedSilverInSession += value;

                RemoveValueFromValuePerHour(_silverPerHourList, ref _silverPerHourValue);
                break;
            case ValueType.FactionPoints:
                _factionPointsPerHourValue += value;
                _factionPointsPerHourList.Add(new ValuePerHour() { DateTime = DateTime.UtcNow, CityFaction = cityFaction, Value = value });
                _currentCityFaction = cityFaction;
                _totalGainedFactionPointsInSession += value;

                RemoveValueFromValuePerHour(_factionPointsPerHourList, ref _factionPointsPerHourValue);
                break;
            case ValueType.Might:
                _mightPerHourValue += value;
                _mightPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalGainedMightInSession += value;

                RemoveValueFromValuePerHour(_mightPerHourList, ref _mightPerHourValue);
                break;
            case ValueType.Favor:
                _favorPerHourValue += value;
                _favorPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalGainedFavorInSession += value;

                RemoveValueFromValuePerHour(_favorPerHourList, ref _favorPerHourValue);
                break;
            case ValueType.PaidSilverForReSpec:
                _paidSilverForReSpecPerHourValue += value;
                _paidSilverForReSpecPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                _totalPaidSilverForReSpecInSession += value;

                RemoveValueFromValuePerHour(_paidSilverForReSpecPerHourList, ref _paidSilverForReSpecPerHourValue);
                break;
        }
        Start();
    }

    public void Start()
    {
        if (_dispatcherTimer.IsEnabled)
        {
            return;
        }

        if (_startTime.Ticks <= 0)
        {
            _startTime = DateTime.UtcNow;
        }

        _dispatcherTimer.Interval = TimeSpan.FromSeconds(2);
        _dispatcherTimer.Tick += UpdateUi;
        _dispatcherTimer.Start();
    }

    public void Stop()
    {
        _dispatcherTimer.Stop();
        _dispatcherTimer.Tick -= UpdateUi;
    }

    public void Reset()
    {
        _startTime = DateTime.UtcNow;
        _mainWindowViewModel.DashboardBindings.Reset();

        var factionValues = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
        if (factionValues != null)
        {
            factionValues.ValuePerHour = 0;
        }

        _totalGainedFameInSession = 0;
        _totalGainedReSpecInSession = 0;
        _totalGainedSilverInSession = 0;
        _totalGainedMightInSession = 0;
        _totalGainedFavorInSession = 0;
        _totalPaidSilverForReSpecInSession = 0;
        _totalGainedFactionPointsInSession = 0;

        _famePerHourValue = 0;
        _reSpecPerHourValue = 0;
        _silverPerHourValue = 0;
        _mightPerHourValue = 0;
        _favorPerHourValue = 0;
        _paidSilverForReSpecPerHourValue = 0;
        _factionPointsPerHourValue = 0;

        _famePerHourList.Clear();
        _reSpecPerHourList.Clear();
        _silverPerHourList.Clear();
        _mightPerHourList.Clear();
        _favorPerHourList.Clear();
        _paidSilverForReSpecPerHourList.Clear();
        _factionPointsPerHourList.Clear();

        UpdateUi(null, EventArgs.Empty);
    }

    private static void RemoveValueFromValuePerHour(ICollection<ValuePerHour> valueList, ref double perHourValue)
    {
        var removeList = valueList?.Where(x => x?.DateTime.AddHours(1) < DateTime.UtcNow).ToList();

        if (removeList == null)
        {
            return;
        }

        foreach (var item in removeList.ToList())
        {
            perHourValue -= item.Value;
            valueList.Remove(item);
        }

        if (perHourValue < 0)
        {
            perHourValue = 0;
        }
    }

    private void UpdateUi(object sender, EventArgs e)
    {
        _mainWindowViewModel.DashboardBindings.TotalGainedFameInSession = _totalGainedFameInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedReSpecPointsInSession = _totalGainedReSpecInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedSilverInSession = _totalGainedSilverInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedMightInSession = _totalGainedMightInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedFavorInSession = _totalGainedFavorInSession;
        _mainWindowViewModel.DashboardBindings.TotalSilverCostForReSpecInSession = _totalPaidSilverForReSpecInSession;

        var totalSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;

        var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
        if (factionPointStat != null)
        {
            factionPointStat.CityFaction = _currentCityFaction;
            factionPointStat.ValuePerHour = _factionPointsPerHourValue.GetValuePerHour(totalSeconds);
            factionPointStat.Value = _totalGainedFactionPointsInSession;
        }

        _mainWindowViewModel.DashboardBindings.FamePerHour = _famePerHourValue.GetValuePerHour(totalSeconds);
        _mainWindowViewModel.DashboardBindings.ReSpecPointsPerHour = _reSpecPerHourValue.GetValuePerHour(totalSeconds);
        _mainWindowViewModel.DashboardBindings.SilverPerHour = _silverPerHourValue.GetValuePerHour(totalSeconds);
        _mainWindowViewModel.DashboardBindings.MightPerHour = _mightPerHourValue.GetValuePerHour(totalSeconds);
        _mainWindowViewModel.DashboardBindings.FavorPerHour = _favorPerHourValue.GetValuePerHour(totalSeconds);
        _mainWindowViewModel.DashboardBindings.SilverCostForReSpecHour = _paidSilverForReSpecPerHourValue.GetValuePerHour(totalSeconds);

        var duration = _startTime - DateTime.UtcNow;
        _mainWindowViewModel.MainTrackerTimer = duration.ToString("hh\\:mm\\:ss");
    }
}