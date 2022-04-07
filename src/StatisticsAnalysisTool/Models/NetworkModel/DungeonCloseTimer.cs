using StatisticsAnalysisTool.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonCloseTimer : INotifyPropertyChanged
    {
        private string _timerString;
        private bool _isDungeonClosed;
        private DateTime _endTime;
        private Visibility _isVisible;

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

        public Visibility IsVisible {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public void UpdateTimer()
        {
            if (IsDungeonClosed)
            {
                return;
            }

            var duration = _endTime - DateTime.UtcNow;
            TimerString = duration.ToString("hh\\:mm\\:ss");

            if (duration.TotalSeconds <= 0)
            {
                IsDungeonClosed = true;
            }
        }

        private void PerformRefreshDungeonTimer()
        {
            _endTime = DateTime.UtcNow.AddSeconds(90);
            IsDungeonClosed = false;
        }

        private ICommand _refreshDungeonTimer;

        public ICommand RefreshDungeonTimer => _refreshDungeonTimer ??= new CommandHandler(PerformRefreshDungeonTimer, true);

        public string TranslationSafe => LanguageController.Translation("SAFE");
        public string TranslationDungeonTimer => LanguageController.Translation("DUNGEON_TIMER");
        public string TranslationResetDungeonTimer => LanguageController.Translation("RESET_DUNGEON_TIMER");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}