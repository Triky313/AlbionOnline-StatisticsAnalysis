using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models
{
    public class MainStatObject : INotifyPropertyChanged
    {
        private CityFaction _cityFaction = CityFaction.Unknown;
        private double _value;
        private double _valuePerHour;
        private Visibility visibility = Visibility.Hidden;

        public CityFaction CityFaction {
            get => _cityFaction;
            set
            {
                _cityFaction = value;
                Visibility = _cityFaction == CityFaction.Unknown ? Visibility.Hidden : Visibility.Visible;
                OnPropertyChanged();
            }
        }

        public double Value {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public double ValuePerHour {
            get => _valuePerHour;
            set
            {
                _valuePerHour = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visibility {
            get => visibility;
            set
            {
                visibility = value;
                OnPropertyChanged();
            }
        }

        public string TranslationTotalFactionPoints => LanguageController.Translation("TOTAL_FACTION_POINTS");

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}