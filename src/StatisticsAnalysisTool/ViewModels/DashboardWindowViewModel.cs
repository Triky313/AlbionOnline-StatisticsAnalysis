using System.Collections.ObjectModel;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DashboardWindowViewModel : INotifyPropertyChanged
    {
        private readonly DashboardWindow _dashboardWindow;
        private DashboardWindowTranslation _translation;
        private DashboardObject _dashboardObject;
        private ObservableCollection<MainStatObject> _factionPointStats;

        public DashboardWindowViewModel(DashboardWindow dashboardWindow, DashboardObject dashboardObject, ObservableCollection<MainStatObject> factionPointStats)
        {
            _dashboardWindow = dashboardWindow;
            DashboardObject = dashboardObject;
            FactionPointStats = factionPointStats;
            Init();
        }

        private void Init()
        {
            Translation = new DashboardWindowTranslation();
        }
        
        public DashboardObject DashboardObject
        {
            get => _dashboardObject;
            set
            {
                _dashboardObject = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MainStatObject> FactionPointStats
        {
            get => _factionPointStats;
            set
            {
                _factionPointStats = value;
                OnPropertyChanged();
            }
        }

        public DashboardWindowTranslation Translation
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