using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Network
{
    public class SilverCountUpTimer : ICountUpTimer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;

        private readonly List<ValuePerHour> _silverPerHourList = new List<ValuePerHour>();
        private bool _isCurrentTimerUpdateActive;
        private double? _lastValue;
        private DateTime _startTime;
        private int? _taskId;
        private double _totalGainedSilver;

        public SilverCountUpTimer(MainWindowViewModel mainWindowViewModel)
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

            var newSilverValue = (double) (value - _lastValue);

            if (newSilverValue == 0) return;

            _lastValue = value;

            _silverPerHourList.Add(new ValuePerHour {DateTime = DateTime.Now, Value = newSilverValue});
            _silverPerHourList.RemoveAll(x => x.DateTime < DateTime.Now.AddHours(-1));

            _totalGainedSilver = _silverPerHourList.Sum(x => x.Value);
            Start();
        }

        public void Start()
        {
            if (_isCurrentTimerUpdateActive) return;

            if (_startTime.Millisecond <= 0) _startTime = DateTime.Now;
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
            _totalGainedSilver = 0;
            _silverPerHourList.Clear();
            CurrentTimerUpdate();
        }

        private void CurrentTimerUpdate()
        {
            if (_isCurrentTimerUpdateActive) return;

            _isCurrentTimerUpdateActive = true;

            var task = Task.Run(async () =>
            {
                while (_isCurrentTimerUpdateActive)
                {
                    _mainWindowViewModel.SilverPerHour = Utilities.GetValuePerHour(_totalGainedSilver, DateTime.Now - _startTime);
                    await Task.Delay(1000);
                }
            });
            _taskId = task.Id;
        }

        private void KillTimerTask(int? taskId)
        {
            if (taskId == null) return;

            try
            {
                if (Process.GetProcesses().Any(x => x.Id == (int) taskId)) Process.GetProcessById((int) taskId).Kill();
            }
            catch (Exception e)
            {
                Log.Error(nameof(KillTimerTask), e);
            }
        }
    }
}