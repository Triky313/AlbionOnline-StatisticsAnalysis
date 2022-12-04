using System;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class MainTabLocationFilterObject : INotifyPropertyChanged
    {
        private bool? _isChecked;
        private string _name;

        public event Action OnCheckedChanged;

        public MainTabLocationFilterObject(MarketLocation location, string name, bool isChecked)
        {
            Location = location;
            Name = name;
            IsChecked = isChecked;
        }

        public MarketLocation Location { get; }

        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnCheckedChanged?.Invoke();
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