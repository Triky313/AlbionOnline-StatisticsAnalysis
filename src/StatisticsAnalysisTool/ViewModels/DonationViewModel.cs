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
    private const long MonthlySilverDonationGoalValue = 27_000_000;
    private const double MonthlyRealMoneyDonationGoalValue = 100d;

    private List<Donation> _donations = [];

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
            .Where(x => !x.IsDonationRealMoney)
            .GroupBy(x => new { x.Contributor, x.Server })
            .Select(arg => new Donation
            {
                Contributor = arg.Key?.Contributor,
                Server = arg.Key?.Server,
                IsDonationRealMoney = arg.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg.FirstOrDefault()?.Timestamp ?? new DateTime(),
                Amount = arg.Sum(x => x?.Amount ?? 0L)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
        SetPlacements(TopDonationsAllTime);

        TopDonationsThisMonth = _donations?
            .Where(x => x?.IsDonationRealMoney == false && x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month)
            .GroupBy(x => new { x.Contributor, x.Server })
            .Select(arg => new Donation
            {
                Contributor = arg.Key?.Contributor,
                Server = arg.Key?.Server,
                IsDonationRealMoney = arg.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg.FirstOrDefault()?.Timestamp ?? new DateTime(),
                Amount = arg.Sum(x => x?.Amount ?? 0L)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
        SetPlacements(TopDonationsThisMonth);

        TopRealMoneyDonations = _donations?
            .Where(x => x.IsDonationRealMoney)
            .GroupBy(x => new { x.Contributor, x.Server })
            .Select(arg => new Donation
            {
                Contributor = arg.Key?.Contributor,
                Server = arg.Key?.Server,
                IsDonationRealMoney = arg.FirstOrDefault()?.IsDonationRealMoney ?? false,
                Timestamp = arg.FirstOrDefault()?.Timestamp ?? new DateTime(),
                RealMoneyAmount = arg.Sum(x => x?.RealMoneyAmount ?? 0d)
            })
            .OrderBy(x => IsAnonymousDonationContributor(x.Contributor))
            .ThenByDescending(x => x.RealMoneyAmount)
            .ToList();
        SetPlacements(TopRealMoneyDonations);

        MonthlySilverDonationAmount = _donations?
            .Where(x => x?.IsDonationRealMoney == false && x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month)
            .Sum(x => x.Amount) ?? 0L;

        MonthlyRealMoneyDonationAmount = _donations?
            .Where(x => x?.IsDonationRealMoney == true && x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month)
            .Sum(x => x.RealMoneyAmount) ?? 0d;

        SetDonationListVisibility();
    }

    private static bool IsAnonymousDonationContributor(string contributor)
    {
        return contributor?.StartsWith("Anonymously", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void SetPlacements(IReadOnlyList<Donation> donations)
    {
        if (donations is null)
        {
            return;
        }

        for (var i = 0; i < donations.Count; i++)
        {
            donations[i].Placement = i + 1;
        }
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
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public List<Donation> TopDonationsThisMonth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public List<Donation> TopRealMoneyDonations
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility DonationsAllTimeVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility NoTopDonationsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility DonationsThisMonthVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility NoDonationsThisMonthVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility TopRealMoneyDonationsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility NoTopRealMoneyDonationsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public DonationTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long MonthlySilverDonationAmount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MonthlySilverDonationProgressText));
        }
    }

    public double MonthlyRealMoneyDonationAmount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MonthlyRealMoneyDonationProgressText));
        }
    }

    public long MonthlySilverDonationGoal => MonthlySilverDonationGoalValue;
    public double MonthlyRealMoneyDonationGoal => MonthlyRealMoneyDonationGoalValue;
    public string MonthlySilverDonationProgressText => $"{MonthlySilverDonationAmount:N0} / {MonthlySilverDonationGoal:N0}";
    public string MonthlyRealMoneyDonationProgressText => $"{MonthlyRealMoneyDonationAmount:N2}\u20AC / {MonthlyRealMoneyDonationGoal:N0}\u20AC";

    public static string PatreonUrl => Settings.Default.PatreonUrl;
    public static string KofiDonationUrl => Settings.Default.KofiDonationUrl;
    public static string DonateUrl => Settings.Default.DonateUrl;
    public static string GitHubSponsorsUrl => Settings.Default.GitHubSponsorsUrl;
}
