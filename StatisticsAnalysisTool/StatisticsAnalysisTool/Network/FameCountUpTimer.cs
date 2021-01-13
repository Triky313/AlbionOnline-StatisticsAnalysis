using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class FameCountUpTimer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _isCurrentTimerUpdateActive;
        private DateTime _startTime;
        private TimeSpan _currentTime;
        private double _totalGainedFame = 0;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private int? _taskId;

        private readonly List<FamePerHourStruct> _famePerHourList = new List<FamePerHourStruct>();

        public FameCountUpTimer(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddFame(double fame)
        {
            _famePerHourList.Add(new FamePerHourStruct() { DateTime = DateTime.Now, Value = fame });
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
            CurrentTimerUpdate();
        }
        
        private void CurrentTimerUpdate()
        {
            _isCurrentTimerUpdateActive = true;

            var task = Task.Run(async () =>
            {
                while (_isCurrentTimerUpdateActive)
                {
                    SetCurrentIntervalTimeForFame();
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
                Process.GetProcessById((int)taskId).Kill();
            }
            catch (Exception e)
            {
                Log.Error(nameof(KillTimerTask), e);
            }
        }

        private void SetCurrentIntervalTimeForFame()
        {
            _currentTime = DateTime.Now - _startTime;
            _mainWindowViewModel.FamePerHour = Formatting.ToStringShort(_totalGainedFame / (_currentTime.TotalSeconds / 60 / 60));
        }

        public struct FamePerHourStruct
        {
            public DateTime DateTime { get; set; }
            public double Value { get; set; }
        }
    }
}