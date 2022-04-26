using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DonationViewModel : INotifyPropertyChanged
    {
        private DonationTranslation _translation;
        private List<Donation> _topDonationsAllTime;
        private List<Donation> _topDonationsThisMonth;
        private Visibility _donationsAllTimeVisibility;
        private Visibility _noTopDonationsVisibility;
        private Visibility _donationsThisMonthVisibility;
        private Visibility _noDonationsThisMonthVisibility;
        private List<Donation> _donations = new();

        public DonationViewModel()
        {
            Translation = new DonationTranslation();
            GetDonations();
            SetDonationsUi();
        }

        public void GetDonations()
        {
            var apiResult = Task.Run(async() => await ApiController.GetDonationsFromJsonAsync()).ConfigureAwait(false);
            _donations = apiResult.GetAwaiter().GetResult();
        }

        public void SetDonationsUi()
        {
            var currentUtc = DateTime.UtcNow;

            TopDonationsAllTime = _donations?
                .GroupBy(x => x?.Contributor)
                .Select(x => x?.First())
                .OrderByDescending(x => x?.Amount)
                .ToList();

            TopDonationsThisMonth = _donations?
                .Where(x => x?.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month)
                .GroupBy(x => x.Contributor)
                .Select(x => x.First())
                .OrderByDescending(x => x?.Amount)
                .ToList();

            if (TopDonationsAllTime?.Count > 0)
            {
                DonationsAllTimeVisibility = Visibility.Visible;
                NoTopDonationsVisibility = Visibility.Collapsed;
            }

            if (TopDonationsThisMonth?.Count > 0)
            {
                DonationsThisMonthVisibility = Visibility.Visible;
                NoDonationsThisMonthVisibility = Visibility.Collapsed;
            }
        }

        public List<Donation> TopDonationsAllTime
        {
            get => _topDonationsAllTime;
            set
            {
                _topDonationsAllTime = value;
                OnPropertyChanged();
            }
        }

        public List<Donation> TopDonationsThisMonth
        {
            get => _topDonationsThisMonth;
            set
            {
                _topDonationsThisMonth = value;
                OnPropertyChanged();
            }
        }

        public Visibility DonationsAllTimeVisibility
        {
            get => _donationsAllTimeVisibility;
            set
            {
                _donationsAllTimeVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility NoTopDonationsVisibility
        {
            get => _noTopDonationsVisibility;
            set
            {
                _noTopDonationsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility DonationsThisMonthVisibility
        {
            get => _donationsThisMonthVisibility;
            set
            {
                _donationsThisMonthVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility NoDonationsThisMonthVisibility
        {
            get => _noDonationsThisMonthVisibility;
            set
            {
                _noDonationsThisMonthVisibility = value;
                OnPropertyChanged();
            }
        }

        public DonationTranslation Translation
        {
            get => _translation;
            set
            {
                _translation = value;
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