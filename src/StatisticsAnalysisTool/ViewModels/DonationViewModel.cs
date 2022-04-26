using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DonationViewModel : INotifyPropertyChanged
    {
        private DonationTranslation _donationTranslation;

        public DonationViewModel()
        {
            DonationTranslation = new DonationTranslation();
        }

        public DonationTranslation DonationTranslation
        {
            get => _donationTranslation;
            set
            {
                _donationTranslation = value;
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