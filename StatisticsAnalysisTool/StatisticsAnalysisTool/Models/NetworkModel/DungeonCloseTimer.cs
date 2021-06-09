using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonCloseTimer : INotifyPropertyChanged
    {
        private string _timerString;
        private bool _isDungeonClosed;
        private readonly DateTime _endTime;

        public DungeonCloseTimer()
        {
            _endTime = DateTime.UtcNow.AddSeconds(90);
        }

        public string TimerString
        {
            get => _timerString;
            set
            {
                _timerString = value;
                OnPropertyChanged();
            }
        }

        public bool IsDungeonClosed
        {
            get => _isDungeonClosed;
            private set
            {
                _isDungeonClosed = value;
                OnPropertyChanged();
            }
        }
        
        public void UpdateTimer()
        {
            var duration = _endTime - DateTime.UtcNow;
            TimerString = duration.ToString("hh\\:mm\\:ss");

            if (duration.Seconds <= 0)
            {
                IsDungeonClosed = true;
            }
        }

        public string TranslationSave => LanguageController.Translation("SAFE");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}