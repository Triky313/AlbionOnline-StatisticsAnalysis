using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DashboardWindowViewModel : INotifyPropertyChanged
    {
        private DashboardBindings _dashboardBindings;
        private ObservableCollection<MainStatObject> _factionPointStats;

        public DashboardWindowViewModel(DashboardBindings dashboardBindings, ObservableCollection<MainStatObject> factionPointStats)
        {
            DashboardBindings = dashboardBindings;
            FactionPointStats = factionPointStats;
        }
        
        public DashboardBindings DashboardBindings
        {
            get => _dashboardBindings;
            set
            {
                _dashboardBindings = value;
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
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}