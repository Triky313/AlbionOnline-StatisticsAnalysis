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

namespace StatisticsAnalysisTool.Network
{
    public class FactionPointsCountUpTimer : ICountUpTimer
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isCurrentTimerUpdateActive;
        private DateTime _startTime;
        private double _totalGainedFactionPoints;
        private CityFaction _currentCityFaction = CityFaction.Unknown;
        private int? _taskId;

        private readonly List<FactionValuePerHour> _factionPointsPerHourList = new List<FactionValuePerHour>();

        public FactionPointsCountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void Add(CityFaction cityFaction, double value)
        {
            _factionPointsPerHourList.Add(new FactionValuePerHour() { DateTime = DateTime.Now, CityFaction = cityFaction, Value = value });
            _factionPointsPerHourList.RemoveAll(x => x.DateTime < DateTime.Now.AddHours(-1));
            _currentCityFaction = cityFaction;

            _totalGainedFactionPoints = _factionPointsPerHourList.Where(x => x.CityFaction == GetNewestEntryFaction(_factionPointsPerHourList)).Sum(x => x.Value);
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
            var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();

            if (factionPointStat != null)
            {
                factionPointStat.CityFaction = CityFaction.Unknown;
                factionPointStat.Value = "0";
                factionPointStat.ValuePerHour = "0";
            }

            _totalGainedFactionPoints = 0;
            _currentCityFaction = CityFaction.Unknown;
            _factionPointsPerHourList.Clear();
            CurrentTimerUpdate();
        }

        private CityFaction GetNewestEntryFaction(List<FactionValuePerHour> list)
        {
            return list?.FirstOrDefault(x => x?.DateTime == list.Max(y => y?.DateTime))?.CityFaction ?? CityFaction.Unknown;
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
                    var factionPointStat = _mainWindowViewModel.FactionPointStats.FirstOrDefault();

                    if (factionPointStat != null)
                    {
                        factionPointStat.CityFaction = _currentCityFaction;
                        factionPointStat.ValuePerHour = Utilities.GetValuePerHour(_totalGainedFactionPoints, DateTime.UtcNow - _startTime);
                        factionPointStat.Value = _totalGainedFactionPoints.ToString("N0");

                    }
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
                if (Process.GetProcesses().Any(x => x.Id == (int)taskId))
                {
                    Process.GetProcessById((int)taskId).Kill();
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(KillTimerTask), e);
            }
        }
    }
}