using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Linq;
using System.Windows.Threading;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network;

public class LiveStatsTracker
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private readonly DispatcherTimer _dispatcherTimer = new();

    private DateTime _startTime;
    private double _totalGainedFameInSession;
    private double _totalGainedReSpecInSession;
    private double _totalGainedSilverInSession;
    private double _totalGainedMightInSession;
    private double _totalGainedFavorInSession;
    private double _totalPaidSilverForReSpecInSession;
    private double _totalGainedFactionPointsInSession;
    private CityFaction _currentCityFaction = CityFaction.Unknown;

    public LiveStatsTracker(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        _startTime = DateTime.UtcNow;
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
                _totalGainedFameInSession += value;
                break;
            case ValueType.ReSpec:
                _totalGainedReSpecInSession += value;
                break;
            case ValueType.Silver:
                _totalGainedSilverInSession += value;
                break;
            case ValueType.FactionPoints:
                _totalGainedFactionPointsInSession += value;
                _currentCityFaction = cityFaction;
                break;
            case ValueType.Might:
                _totalGainedMightInSession += value;
                break;
            case ValueType.Favor:
                _totalGainedFavorInSession += value;
                break;
            case ValueType.PaidSilverForReSpec:
                _totalPaidSilverForReSpecInSession += value;
                break;
        }

        Start();
    }

    public void Start()
    {
        if (_dispatcherTimer.IsEnabled) return;

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

        UpdateUi(null, EventArgs.Empty);
    }

    private void UpdateUi(object sender, EventArgs e)
    {
        var totalSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
        var totalHours = totalSeconds / 3600;

        _mainWindowViewModel.DashboardBindings.TotalGainedFameInSession = _totalGainedFameInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedReSpecPointsInSession = _totalGainedReSpecInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedSilverInSession = _totalGainedSilverInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedMightInSession = _totalGainedMightInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedFavorInSession = _totalGainedFavorInSession;
        _mainWindowViewModel.DashboardBindings.TotalSilverCostForReSpecInSession = _totalPaidSilverForReSpecInSession;

        var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
        if (factionPointStat != null)
        {
            factionPointStat.CityFaction = _currentCityFaction;
            factionPointStat.ValuePerHour = _totalGainedFactionPointsInSession / totalHours;
            factionPointStat.Value = _totalGainedFactionPointsInSession;
        }

        _mainWindowViewModel.DashboardBindings.FamePerHour = _totalGainedFameInSession / totalHours;
        _mainWindowViewModel.DashboardBindings.ReSpecPointsPerHour = _totalGainedReSpecInSession / totalHours;
        _mainWindowViewModel.DashboardBindings.SilverPerHour = _totalGainedSilverInSession / totalHours;
        _mainWindowViewModel.DashboardBindings.MightPerHour = _totalGainedMightInSession / totalHours;
        _mainWindowViewModel.DashboardBindings.FavorPerHour = _totalGainedFavorInSession / totalHours;
        _mainWindowViewModel.DashboardBindings.SilverCostForReSpecHour = _totalPaidSilverForReSpecInSession / totalHours;

        var duration = DateTime.UtcNow - _startTime;
        _mainWindowViewModel.MainTrackerTimer = duration.ToString("hh\\:mm\\:ss");
    }
}