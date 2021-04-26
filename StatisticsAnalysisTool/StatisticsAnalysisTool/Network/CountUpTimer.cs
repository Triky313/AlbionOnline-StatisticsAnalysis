using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network
{
    public class CountUpTimer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MainWindowViewModel _mainWindowViewModel;

        private readonly List<ValuePerHour> _famePerHourList = new List<ValuePerHour>();
        private readonly List<ValuePerHour> _reSpecPerHourList = new List<ValuePerHour>();
        private readonly List<ValuePerHour> _silverPerHourList = new List<ValuePerHour>();
        private readonly List<ValuePerHour> _factionPointsPerHourList = new List<ValuePerHour>();

        private double _famePerHourValue;
        private double _reSpecPerHourValue;
        private double _silverPerHourValue;
        private double _factionPointsPerHourValue;

        private bool _isCurrentTimerUpdateActive;
        private DateTime _startTime;
        private int? _taskId;
        private double _totalGainedFameInSession;
        private double _totalGainedReSpecInSession;
        private double _totalGainedSilverInSession;
        private double _totalGainedFactionPointsInSession;
        private CityFaction _currentCityFaction = CityFaction.Unknown;
        private double? _lastReSpecValue;

        public CountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void Add(ValueType valueType, double value, CityFaction cityFaction = CityFaction.Unknown)
        {
            switch (valueType)
            {
                case ValueType.Fame:
                    _famePerHourValue += value;
                    _famePerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                    _totalGainedFameInSession += value;

                    RemoveValueFromValuePerHour(_famePerHourList, _famePerHourValue);
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

                    RemoveValueFromValuePerHour(_reSpecPerHourList, _reSpecPerHourValue);
                    break;
                case ValueType.Silver:
                    _silverPerHourValue += value;
                    _silverPerHourList.Add(new ValuePerHour { DateTime = DateTime.UtcNow, Value = value });
                    _totalGainedSilverInSession += value;

                    RemoveValueFromValuePerHour(_silverPerHourList, _silverPerHourValue);
                    break;
                case ValueType.FactionPoints:
                    _factionPointsPerHourValue += value;
                    _factionPointsPerHourList.Add(new ValuePerHour() { DateTime = DateTime.UtcNow, CityFaction = cityFaction, Value = value });
                    _currentCityFaction = cityFaction;
                    _totalGainedFactionPointsInSession += value;

                    RemoveValueFromValuePerHour(_factionPointsPerHourList, _factionPointsPerHourValue);
                    break;
            }
            Start();
        }

        public void Start()
        {
            if (_isCurrentTimerUpdateActive)
            {
                return;
            }

            if (_startTime.Millisecond <= 0)
            {
                _startTime = DateTime.UtcNow;
            }

            CurrentTimerUpdate();
        }

        public void Stop()
        {
            _isCurrentTimerUpdateActive = false;
            KillTimerTask(_taskId);
        }

        public void Reset()
        {
            _startTime = DateTime.UtcNow;
            _mainWindowViewModel.FamePerHour = "0";
            _mainWindowViewModel.ReSpecPointsPerHour = "0";
            _mainWindowViewModel.SilverPerHour = "0";

            var factionValues = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
            if (factionValues != null)
            {
                factionValues.ValuePerHour = "0";
            }

            _totalGainedFameInSession = 0;
            _totalGainedReSpecInSession = 0;
            _totalGainedSilverInSession = 0;
            _totalGainedFactionPointsInSession = 0;

            _famePerHourValue = 0;
            _reSpecPerHourValue = 0;
            _silverPerHourValue = 0;
            _factionPointsPerHourValue = 0;

            _famePerHourList.Clear();
            _reSpecPerHourList.Clear();
            _silverPerHourList.Clear();
            _factionPointsPerHourList.Clear();

            CurrentTimerUpdate();
        }

        private void RemoveValueFromValuePerHour(List<ValuePerHour> valueList, double perHourValue)
        {
            var removeList = valueList.Where(x => x.DateTime < DateTime.UtcNow.AddHours(-1));

            foreach (var item in removeList)
            {
                perHourValue -= item.Value;

                if (perHourValue < 0)
                {
                    perHourValue = 0;
                }

                valueList.Remove(item);
            }

            valueList.RemoveAll(x => x.DateTime < DateTime.UtcNow.AddHours(-1));
        }

        private void CurrentTimerUpdate()
        {
            if (_isCurrentTimerUpdateActive)
            {
                return;
            }

            _isCurrentTimerUpdateActive = true;

            var task = Task.Run(async () =>
            {
                while (_isCurrentTimerUpdateActive)
                {
                    _mainWindowViewModel.TotalGainedFameInSession = _totalGainedFameInSession.ToString("N0", LanguageController.CurrentCultureInfo);
                    _mainWindowViewModel.TotalGainedReSpecPointsInSession = _totalGainedReSpecInSession.ToString("N0", LanguageController.CurrentCultureInfo);
                    _mainWindowViewModel.TotalGainedSilverInSession = _totalGainedSilverInSession.ToString("N0", LanguageController.CurrentCultureInfo);

                    var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();
                    if (factionPointStat != null)
                    {
                        factionPointStat.CityFaction = _currentCityFaction;
                        factionPointStat.ValuePerHour = Utilities.GetValuePerHourInShort(_factionPointsPerHourValue, DateTime.UtcNow - _startTime);
                        factionPointStat.Value = _totalGainedFactionPointsInSession.ToString("N0", LanguageController.CurrentCultureInfo);

                    }

                    _mainWindowViewModel.FamePerHour = Utilities.GetValuePerHourInShort(_famePerHourValue, DateTime.UtcNow - _startTime);
                    _mainWindowViewModel.ReSpecPointsPerHour = Utilities.GetValuePerHourInShort(_reSpecPerHourValue, DateTime.UtcNow - _startTime);
                    _mainWindowViewModel.SilverPerHour = Utilities.GetValuePerHourInShort(_silverPerHourValue, DateTime.UtcNow - _startTime);

                    var duration = _startTime - DateTime.UtcNow;
                    _mainWindowViewModel.MainTrackerTimer = duration.ToString("hh\\:mm\\:ss");
                    await Task.Delay(1000);
                }
            });
            _taskId = task.Id;
        }

        private void KillTimerTask(int? taskId)
        {
            if (taskId == null)
            {
                return;
            }

            try
            {
                if (Process.GetProcesses().Any(x => x.Id == (int) taskId))
                {
                    Process.GetProcessById((int) taskId).Kill();
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(KillTimerTask), e);
            }
        }
    }
}