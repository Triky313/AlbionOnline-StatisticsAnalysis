using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class CityFilterObject : INotifyPropertyChanged
    {
        private bool? _isSelected;
        private string _name;

        public CityFilterObject(MarketLocation location, string name, bool isSelected)
        {
            Location = location;
            Name = name;
            IsSelected = isSelected;
        }

        public MarketLocation Location { get; }

        public bool? IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}