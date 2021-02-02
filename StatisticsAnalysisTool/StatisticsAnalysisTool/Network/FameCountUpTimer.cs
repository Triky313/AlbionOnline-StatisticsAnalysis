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
    public class FameCountUpTimer : ICountUpTimer
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isCurrentTimerUpdateActive;
        private DateTime _startTime;
        private double _totalGainedFame;
        private int? _taskId;

        private readonly List<ValuePerHour> _famePerHourList = new List<ValuePerHour>();

        public FameCountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void Add(double value)
        {
            _famePerHourList.Add(new ValuePerHour() { DateTime = DateTime.Now, Value = value });
            _famePerHourList.RemoveAll(x => x.DateTime < DateTime.Now.AddHours(-1));

            _totalGainedFame = _famePerHourList.Sum(x => x.Value);
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
            _mainWindowViewModel.FamePerHour = "0";
            _totalGainedFame = 0;
            _famePerHourList.Clear();
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
                    _mainWindowViewModel.FamePerHour = Utilities.GetValuePerHour(_totalGainedFame, DateTime.Now - _startTime);
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