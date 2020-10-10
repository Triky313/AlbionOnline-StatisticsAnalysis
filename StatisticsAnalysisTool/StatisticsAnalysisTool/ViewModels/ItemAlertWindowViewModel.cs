using StatisticsAnalysisTool.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class ItemAlertWindowViewModel : INotifyPropertyChanged
    {
        private readonly AlertInfos _alertInfos;
        private ItemAlertWindowTranslation _translation;

        public ItemAlertWindowViewModel(AlertInfos alertInfos)
        {
            _alertInfos = alertInfos;
            Init();
        }

        private void Init()
        {
            Translation = new ItemAlertWindowTranslation(_alertInfos.Item);
        }

        #region Bindings

        public ItemAlertWindowTranslation Translation {
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

        #endregion

    }
}