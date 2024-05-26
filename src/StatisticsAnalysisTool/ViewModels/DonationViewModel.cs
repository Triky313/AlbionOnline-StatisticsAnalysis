using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class DonationViewModel : BaseViewModel
{
    private DonationTranslation _translation;
    private List<Donation> _topDonationsAllTime;
    private List<Donation> _topDonationsThisMonth;
    private Visibility _donationsAllTimeVisibility = Visibility.Collapsed;
    private Visibility _noTopDonationsVisibility = Visibility.Visible;
    private Visibility _donationsThisMonthVisibility = Visibility.Collapsed;
    private Visibility _noDonationsThisMonthVisibility = Visibility.Visible;
    private List<Donation> _donations = new();
    private List<Donation> _topRealMoneyDonations;
    private Visibility _topRealMoneyDonationsVisibility = Visibility.Collapsed;
    private Visibility _noTopRealMoneyDonationsVisibility = Visibility.Visible;

    public DonationViewModel()
    {
        Translation = new DonationTranslation();
        GetDonations();
        SetDonationsUi();
    }

    public void GetDonations()
    {
        var apiResult = Task.Run(ApiController.GetDonationsFromJsonAsync).ConfigureAwait(false);
        _donations = apiResult.GetAwaiter().GetResult();
    }

    public void SetDonationsUi()
    {
        var currentUtc = DateTime.UtcNow;

        TopDonationsAllTime = _donations?
            .Where(x => x.IsDonationRealMoney == false)
            .GroupBy(x => new { x?.Contributor, x?.Server })
            .Select(arg => new Donation
            {
                Contributor = arg?.Key?.Contributor,
                Server = arg?.Key?.Server,
                IsDonationRealMoney = arg?.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg?.FirstOrDefault()?.Timestamp ?? new DateTime(),
                Amount = arg?.Sum(x => x?.Amount ?? 0L) is not null ? arg.Sum(x => x?.Amount ?? 0L) : 0L
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        TopDonationsThisMonth = _donations?
            .Where(x => x?.IsDonationRealMoney == false && x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month)
            .GroupBy(x => new { x?.Contributor, x?.Server })
            .Select(arg => new Donation
            {
                Contributor = arg?.Key?.Contributor,
                Server = arg?.Key?.Server,
                IsDonationRealMoney = arg?.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg?.FirstOrDefault()?.Timestamp ?? new DateTime(),
                Amount = arg?.Sum(x => x?.Amount ?? 0L) is not null ? arg.Sum(x => x?.Amount ?? 0L) : 0L
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        TopRealMoneyDonations = _donations?
            .Where(x => x.IsDonationRealMoney)
            .GroupBy(x => new { x?.Contributor, x?.Server })
            .Select(arg => new Donation
            {
                Contributor = arg?.Key?.Contributor,
                Server = arg?.Key?.Server,
                IsDonationRealMoney = arg?.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg?.FirstOrDefault()?.Timestamp ?? new DateTime(),
                RealMoneyAmount = arg?.Sum(x => x?.RealMoneyAmount ?? 0d) is not null ? arg.Sum(x => x?.RealMoneyAmount ?? 0d) : 0d
            })
            .OrderByDescending(x => x.RealMoneyAmount)
            .ToList();

        SetDonationListVisibility();
    }

    private void SetDonationListVisibility()
    {
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

        if (TopRealMoneyDonations?.Count > 0)
        {
            TopRealMoneyDonationsVisibility = Visibility.Visible;
            NoTopRealMoneyDonationsVisibility = Visibility.Collapsed;
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

    public List<Donation> TopRealMoneyDonations
    {
        get => _topRealMoneyDonations;
        set
        {
            _topRealMoneyDonations = value;
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

    public Visibility TopRealMoneyDonationsVisibility
    {
        get => _topRealMoneyDonationsVisibility;
        set
        {
            _topRealMoneyDonationsVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility NoTopRealMoneyDonationsVisibility
    {
        get => _noTopRealMoneyDonationsVisibility;
        set
        {
            _noTopRealMoneyDonationsVisibility = value;
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

    public static string PatreonUrl => Settings.Default.PatreonUrl;
    public static string DonateUrl => Settings.Default.DonateUrl;
    public static string GitHubSponsorsUrl => Settings.Default.GitHubSponsorsUrl;
}