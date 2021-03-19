using log4net;
using StatisticsAnalysisTool.Common;
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
        private int? _taskId;

        private readonly List<ValuePerHour> _factionPointsPerHourList = new List<ValuePerHour>();

        public FactionPointsCountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void Add(double value)
        {
            _factionPointsPerHourList.Add(new ValuePerHour() { DateTime = DateTime.Now, Value = value });
            _factionPointsPerHourList.RemoveAll(x => x.DateTime < DateTime.Now.AddHours(-1));

            _totalGainedFactionPoints = _factionPointsPerHourList.Sum(x => x.Value);
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
                _startTime = DateTime.Now;
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
            _startTime = DateTime.Now;
            _mainWindowViewModel.FactionPointsPerHour = "0";
            _totalGainedFactionPoints = 0;
            _factionPointsPerHourList.Clear();
            CurrentTimerUpdate();
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
                    _mainWindowViewModel.FactionPointsPerHour = Utilities.GetValuePerHour(_totalGainedFactionPoints, DateTime.Now - _startTime);
                    _mainWindowViewModel.TotalGainedFactionPoints = _totalGainedFactionPoints.ToString("N2");
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