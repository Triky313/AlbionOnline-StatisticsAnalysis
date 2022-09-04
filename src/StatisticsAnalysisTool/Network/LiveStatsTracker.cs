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

namespace StatisticsAnalysisTool.Network
{
    public class LiveStatsTracker
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        private readonly List<ValuePerHour> _famePerHourList = new();
        private readonly List<ValuePerHour> _reSpecPerHourList = new();
        private readonly List<ValuePerHour> _silverPerHourList = new();
        private readonly List<ValuePerHour> _mightPerHourList = new();
        private readonly List<ValuePerHour> _favorPerHourList = new();
        private readonly List<ValuePerHour> _factionPointsPerHourList = new();
        private readonly List<ValuePerHour> _reSpecPerHourWithReSpecBoostList = new();

        private double _famePerHourValue;
        private double _reSpecPerHourValue;
        private double _silverPerHourValue;
        private double _mightPerHourValue;
        private double _favorPerHourValue;
        private double _factionPointsPerHourValue;
        private double _reSpecPerHourValueWithReSpecBoost;

        private DateTime _startTime;
        private double _totalGainedFameInSession;
        private double _totalGainedReSpecInSession;
        private double _totalGainedSilverInSession;
        private double _totalGainedMightInSession;
        private double _totalGainedFavorInSession;
        private double _totalGainedFactionPointsInSession;
        private double _totalGainedReSpecInSessionWithReSpecBoost;
        private CityFaction _currentCityFaction = CityFaction.Unknown;
        private double? _lastReSpecValue;
        private readonly TrackingController _trackingController;
        private readonly DispatcherTimer _dispatcherTimer = new();

        private const double SilverCostPerFameCredit = 0.75d;

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
                    var internalReSpecValue = Utilities.AddValue(value, _lastReSpecValue, out _lastReSpecValue);
                    if (internalReSpecValue <= 0)
                    {
                        break;
                    }

                    _reSpecPerHourValue += internalReSpecValue;
                    _reSpecPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                    _totalGainedReSpecInSession += internalReSpecValue;

                    RemoveValueFromValuePerHour(_reSpecPerHourList, ref _reSpecPerHourValue);

                    if (_trackingController.EntityController.LocalUserData.IsReSpecActive)
                    {
                        _reSpecPerHourValueWithReSpecBoost += internalReSpecValue;
                        _reSpecPerHourWithReSpecBoostList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                        _totalGainedReSpecInSessionWithReSpecBoost += internalReSpecValue;

                        RemoveValueFromValuePerHour(_reSpecPerHourWithReSpecBoostList, ref _reSpecPerHourValueWithReSpecBoost);
                    }
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
            _totalGainedFactionPointsInSession = 0;
            _totalGainedReSpecInSessionWithReSpecBoost = 0;

            _famePerHourValue = 0;
            _reSpecPerHourValue = 0;
            _silverPerHourValue = 0;
            _mightPerHourValue = 0;
            _favorPerHourValue = 0;
            _factionPointsPerHourValue = 0;
            _reSpecPerHourValueWithReSpecBoost = 0;

            _famePerHourList.Clear();
            _reSpecPerHourList.Clear();
            _silverPerHourList.Clear();
            _mightPerHourList.Clear();
            _favorPerHourList.Clear();
            _factionPointsPerHourList.Clear();
            _reSpecPerHourWithReSpecBoostList.Clear();

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

            _mainWindowViewModel.DashboardBindings.TotalSilverCostForReSpecInSession = _totalGainedReSpecInSessionWithReSpecBoost * SilverCostPerFameCredit;

            var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
            if (factionPointStat != null)
            {
                factionPointStat.CityFaction = _currentCityFaction;
                factionPointStat.ValuePerHour = Utilities.GetValuePerHourToDouble(_factionPointsPerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);
                factionPointStat.Value = _totalGainedFactionPointsInSession;
            }

            _mainWindowViewModel.DashboardBindings.FamePerHour = Utilities.GetValuePerHourToDouble(_famePerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);
            _mainWindowViewModel.DashboardBindings.ReSpecPointsPerHour = Utilities.GetValuePerHourToDouble(_reSpecPerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);
            _mainWindowViewModel.DashboardBindings.SilverPerHour = Utilities.GetValuePerHourToDouble(_silverPerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);
            _mainWindowViewModel.DashboardBindings.MightPerHour = Utilities.GetValuePerHourToDouble(_mightPerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);
            _mainWindowViewModel.DashboardBindings.FavorPerHour = Utilities.GetValuePerHourToDouble(_favorPerHourValue, (DateTime.UtcNow - _startTime).TotalSeconds);

            _mainWindowViewModel.DashboardBindings.SilverCostForReSpecHour = Utilities.GetValuePerHourToDouble(_reSpecPerHourValueWithReSpecBoost * SilverCostPerFameCredit, (DateTime.UtcNow - _startTime).TotalSeconds);

            var duration = _startTime - DateTime.UtcNow;
            _mainWindowViewModel.MainTrackerTimer = duration.ToString("hh\\:mm\\:ss");
        }
    }
}