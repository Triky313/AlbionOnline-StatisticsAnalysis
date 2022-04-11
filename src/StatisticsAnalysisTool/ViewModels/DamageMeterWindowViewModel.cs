using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DamageMeterWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DamageMeterFragment> _damageMeter;
        private DamageMeterWindowTranslation _translation;

        public DamageMeterWindowViewModel(ObservableCollection<DamageMeterFragment> damageMeter)
        {
            DamageMeter = damageMeter;
            Init();
        }

        private void Init()
        {
            Translation = new DamageMeterWindowTranslation();
        }

        public ObservableCollection<DamageMeterFragment> DamageMeter
        {
            get => _damageMeter;
            set
            {
                _damageMeter = value;
                OnPropertyChanged();
            }
        }

        public DamageMeterWindowTranslation Translation
        {
            get => _translation;
            set
            {
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