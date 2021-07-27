using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DamageMeterWindowViewModel : INotifyPropertyChanged
    {
        private readonly DamageMeterWindow _damageMeterWindow;
        private ObservableCollection<DamageMeterFragment> _damageMeter;
        private DamageMeterWindowTranslation _translation;

        public DamageMeterWindowViewModel(DamageMeterWindow damageMeterWindow, ObservableCollection<DamageMeterFragment> damageMeter)
        {
            _damageMeterWindow = damageMeterWindow;
            DamageMeter = damageMeter;
            Init();
        }

        private void Init()
        {
            Translation = new DamageMeterWindowTranslation();
        }

        public void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = _damageMeterWindow.Width;
            var windowHeight = _damageMeterWindow.Height;
            _damageMeterWindow.Left = screenWidth / 2 - windowWidth / 2;
            _damageMeterWindow.Top = screenHeight / 2 - windowHeight / 2;
        }

        public ObservableCollection<DamageMeterFragment> DamageMeter {
            get => _damageMeter;
            set {
                _damageMeter = value;
                OnPropertyChanged();
            }
        }

        public DamageMeterWindowTranslation Translation {
            get => _translation;
            set {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}