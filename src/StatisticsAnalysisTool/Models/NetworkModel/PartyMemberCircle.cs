using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class PartyMemberCircle : INotifyPropertyChanged
    {
        public Guid UserGuid { get; set; }

        private string _name;
        private string _weaponCategoryId;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        
        public string WeaponCategoryId {
            get => _weaponCategoryId;
            set
            {
                _weaponCategoryId = value;
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