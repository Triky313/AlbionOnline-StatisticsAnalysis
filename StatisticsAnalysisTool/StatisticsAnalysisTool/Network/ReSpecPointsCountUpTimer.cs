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
    public class ReSpecPointsCountUpTimer : ICountUpTimer
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isCurrentTimerUpdateActive;
        private DateTime _startTime;
        private double _totalGained;
        private int? _taskId;
        private double? _lastValue;

        private readonly List<ValuePerHour> _valuePerHourList = new List<ValuePerHour>();

        public ReSpecPointsCountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void Add(double value)
        {
            if (_lastValue == null)
            {
                _lastValue = value;
                return;
            }

            var newValue = (double)(value - _lastValue);

            if (newValue == 0)
            {
                return;
            }

            _lastValue = value;

            _valuePerHourList.Add(new ValuePerHour() { DateTime = DateTime.Now, Value = newValue });
            _valuePerHourList.RemoveAll(x => x.DateTime < DateTime.Now.AddHours(-1));

            _totalGained = _valuePerHourList.Sum(x => x.Value);
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
            _mainWindowViewModel.SilverPerHour = "0";
            _totalGained = 0;
            _lastValue = null;
            _valuePerHourList.Clear();
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
                    _mainWindowViewModel.FamePerHour = Utilities.GetValuePerHour(_totalGained, DateTime.Now - _startTime);
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