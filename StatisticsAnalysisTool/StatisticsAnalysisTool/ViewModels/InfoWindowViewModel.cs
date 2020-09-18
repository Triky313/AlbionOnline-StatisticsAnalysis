using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    internal class InfoWindowViewModel : INotifyPropertyChanged
    {
        private bool _showNotAgainChecked;
        private InfoWindowTranslation _translation;

        public InfoWindowViewModel()
        {
            Init();
        }

        private void Init()
        {
            Translation = new InfoWindowTranslation();
        }

        #region Bindings

        public InfoWindowTranslation Translation {
            get => _translation;
            set {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public bool ShowNotAgainChecked {
            get => _showNotAgainChecked;
            set {
                _showNotAgainChecked = value;
                Settings.Default.ShowInfoWindowOnStartChecked = !_showNotAgainChecked;
                OnPropertyChanged();
            }
        }

        public string DonateUrl => Settings.Default.DonateUrl;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Bindings
    }
}