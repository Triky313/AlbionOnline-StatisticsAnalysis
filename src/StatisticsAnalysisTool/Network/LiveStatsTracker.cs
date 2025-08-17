using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network;

public class LiveStatsTracker
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private readonly DispatcherTimer _dispatcherTimer = new();

    private sealed class SlidingWindow
    {
        private readonly Queue<Entry> _q = new();
        private readonly Lock _gate = new();
        private double _sum;

        private readonly TimeSpan _window = TimeSpan.FromHours(1);

        private readonly struct Entry(DateTime ts, double val)
        {
            public DateTime Timestamp { get; } = ts;
            public double Value { get; } = val;
        }

        public void Clear()
        {
            lock (_gate)
            {
                _q.Clear();
                _sum = 0;
            }
        }

        public void Add(double value, DateTime nowUtc)
        {
            lock (_gate)
            {
                _q.Enqueue(new Entry(nowUtc, value));
                _sum += value;
                PurgeOld_NoLock(nowUtc);
            }
        }

        public (double Sum, double WindowSeconds) Snapshot(DateTime nowUtc)
        {
            lock (_gate)
            {
                PurgeOld_NoLock(nowUtc);
                if (_q.Count == 0)
                {
                    return (0d, 0d);
                }

                var oldest = _q.Peek().Timestamp;
                var seconds = Math.Max(0, Math.Min(_window.TotalSeconds, (nowUtc - oldest).TotalSeconds));

                if (seconds < 1)
                {
                    seconds = 1;
                }

                return (_sum, seconds);
            }
        }

        private void PurgeOld_NoLock(DateTime nowUtc)
        {
            var cutoff = nowUtc - _window;
            while (_q.Count > 0 && _q.Peek().Timestamp < cutoff)
            {
                _sum -= _q.Dequeue().Value;
            }
        }
    }

    private readonly SlidingWindow _fameWin = new();
    private readonly SlidingWindow _respecWin = new();
    private readonly SlidingWindow _silverWin = new();
    private readonly SlidingWindow _mightWin = new();
    private readonly SlidingWindow _favorWin = new();
    private readonly SlidingWindow _respecCostWin = new();
    private readonly SlidingWindow _factionPointsWin = new();

    private DateTime _sessionStartUtc;
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
        _sessionStartUtc = DateTime.UtcNow;

        _dispatcherTimer.Interval = TimeSpan.FromSeconds(2);
        _dispatcherTimer.Tick += UpdateUi;
    }

    public void Add(ValueType valueType, double value, CityFaction cityFaction = CityFaction.Unknown)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var now = DateTime.UtcNow;

        switch (valueType)
        {
            case ValueType.Fame:
                _totalGainedFameInSession += value;
                _fameWin.Add(value, now);
                break;

            case ValueType.ReSpec:
                _totalGainedReSpecInSession += value;
                _respecWin.Add(value, now);
                break;

            case ValueType.Silver:
                _totalGainedSilverInSession += value;
                _silverWin.Add(value, now);
                break;

            case ValueType.FactionPoints:
                _totalGainedFactionPointsInSession += value;
                _factionPointsWin.Add(value, now);
                _currentCityFaction = cityFaction;
                break;

            case ValueType.Might:
                _totalGainedMightInSession += value;
                _mightWin.Add(value, now);
                break;

            case ValueType.Favor:
                _totalGainedFavorInSession += value;
                _favorWin.Add(value, now);
                break;

            case ValueType.PaidSilverForReSpec:
                _totalPaidSilverForReSpecInSession += value;
                _respecCostWin.Add(value, now);
                break;
        }

        Start();
    }

    public void Start()
    {
        if (!_dispatcherTimer.IsEnabled)
        {
            _dispatcherTimer.Start();
        }
    }

    public void Stop()
    {
        if (_dispatcherTimer.IsEnabled)
        {
            _dispatcherTimer.Stop();
        }
    }

    public void Reset()
    {
        _sessionStartUtc = DateTime.UtcNow;

        _fameWin.Clear();
        _respecWin.Clear();
        _silverWin.Clear();
        _mightWin.Clear();
        _favorWin.Clear();
        _respecCostWin.Clear();
        _factionPointsWin.Clear();

        _totalGainedFameInSession = 0;
        _totalGainedReSpecInSession = 0;
        _totalGainedSilverInSession = 0;
        _totalGainedMightInSession = 0;
        _totalGainedFavorInSession = 0;
        _totalPaidSilverForReSpecInSession = 0;
        _totalGainedFactionPointsInSession = 0;
        _currentCityFaction = CityFaction.Unknown;

        _mainWindowViewModel.DashboardBindings.Reset();

        var factionValues = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
        if (factionValues != null)
        {
            factionValues.ValuePerHour = 0;
            factionValues.Value = 0;
            factionValues.CityFaction = CityFaction.Unknown;
        }

        UpdateUi(null, EventArgs.Empty);
    }

    private void UpdateUi(object sender, EventArgs e)
    {
        var now = DateTime.UtcNow;

        // Session totals
        _mainWindowViewModel.DashboardBindings.TotalGainedFameInSession = _totalGainedFameInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedReSpecPointsInSession = _totalGainedReSpecInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedSilverInSession = _totalGainedSilverInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedMightInSession = _totalGainedMightInSession;
        _mainWindowViewModel.DashboardBindings.TotalGainedFavorInSession = _totalGainedFavorInSession;
        _mainWindowViewModel.DashboardBindings.TotalSilverCostForReSpecInSession = _totalPaidSilverForReSpecInSession;

        // Sliding Window
        var (fameSum, fameSec) = _fameWin.Snapshot(now);
        var (respecSum, respecSec) = _respecWin.Snapshot(now);
        var (silverSum, silverSec) = _silverWin.Snapshot(now);
        var (mightSum, mightSec) = _mightWin.Snapshot(now);
        var (favorSum, favorSec) = _favorWin.Snapshot(now);
        var (respecCostSum, respecCostSec) = _respecCostWin.Snapshot(now);
        var (factionSum, factionSec) = _factionPointsWin.Snapshot(now);

        // Per hour
        _mainWindowViewModel.DashboardBindings.FamePerHour = fameSum * 3600.0 / fameSec;
        _mainWindowViewModel.DashboardBindings.ReSpecPointsPerHour = respecSum * 3600.0 / respecSec;
        _mainWindowViewModel.DashboardBindings.SilverPerHour = silverSum * 3600.0 / silverSec;
        _mainWindowViewModel.DashboardBindings.MightPerHour = mightSum * 3600.0 / mightSec;
        _mainWindowViewModel.DashboardBindings.FavorPerHour = favorSum * 3600.0 / favorSec;
        _mainWindowViewModel.DashboardBindings.SilverCostForReSpecHour = respecCostSum * 3600.0 / respecCostSec;

        var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
        if (factionPointStat != null)
        {
            factionPointStat.CityFaction = _currentCityFaction;
            factionPointStat.ValuePerHour = factionSum * 3600.0 / factionSec;
            factionPointStat.Value = _totalGainedFactionPointsInSession;
        }

        // Session-Timer
        var duration = now - _sessionStartUtc;
        _mainWindowViewModel.MainTrackerTimer = duration.ToString("hh\\:mm\\:ss");
    }
}