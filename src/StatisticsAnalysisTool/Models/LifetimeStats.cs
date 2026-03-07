using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

/// <summary>
/// Persistent aggregated stats across configurable timeframes, derived from stored DailyValues.
/// </summary>
public class LifetimeStats : BaseViewModel
{
    #region Backing fields – Today

    private double _fameToday;
    private double _silverToday;
    private double _reSpecToday;
    private double _mightToday;
    private double _favorToday;
    private double _factionPointsToday;
    private double _gatheringValueToday;

    #endregion

    #region Backing fields – This Week

    private double _fameThisWeek;
    private double _silverThisWeek;
    private double _reSpecThisWeek;
    private double _mightThisWeek;
    private double _favorThisWeek;
    private double _factionPointsThisWeek;
    private double _gatheringValueThisWeek;

    #endregion

    #region Backing fields – Last Week

    private double _fameLastWeek;
    private double _silverLastWeek;
    private double _reSpecLastWeek;
    private double _mightLastWeek;
    private double _favorLastWeek;
    private double _factionPointsLastWeek;
    private double _gatheringValueLastWeek;

    #endregion

    #region Backing fields – This Month

    private double _fameThisMonth;
    private double _silverThisMonth;
    private double _reSpecThisMonth;
    private double _mightThisMonth;
    private double _favorThisMonth;
    private double _factionPointsThisMonth;
    private double _gatheringValueThisMonth;

    #endregion

    #region Backing fields – Last Month

    private double _fameLastMonth;
    private double _silverLastMonth;
    private double _reSpecLastMonth;
    private double _mightLastMonth;
    private double _favorLastMonth;
    private double _factionPointsLastMonth;
    private double _gatheringValueLastMonth;

    #endregion

    #region Backing fields – This Year

    private double _fameThisYear;
    private double _silverThisYear;
    private double _reSpecThisYear;
    private double _mightThisYear;
    private double _favorThisYear;
    private double _factionPointsThisYear;
    private double _gatheringValueThisYear;

    #endregion

    #region Backing fields – Total

    private double _fameTotal;
    private double _silverTotal;
    private double _reSpecTotal;
    private double _mightTotal;
    private double _favorTotal;
    private double _factionPointsTotal;
    private double _gatheringValueTotal;

    #endregion

    #region Today

    public double FameToday
    {
        get => _fameToday;
        set { _fameToday = value; OnPropertyChanged(); }
    }

    public double SilverToday
    {
        get => _silverToday;
        set { _silverToday = value; OnPropertyChanged(); }
    }

    public double ReSpecToday
    {
        get => _reSpecToday;
        set { _reSpecToday = value; OnPropertyChanged(); }
    }

    public double MightToday
    {
        get => _mightToday;
        set { _mightToday = value; OnPropertyChanged(); }
    }

    public double FavorToday
    {
        get => _favorToday;
        set { _favorToday = value; OnPropertyChanged(); }
    }

    public double FactionPointsToday
    {
        get => _factionPointsToday;
        set { _factionPointsToday = value; OnPropertyChanged(); }
    }

    public double GatheringValueToday
    {
        get => _gatheringValueToday;
        set { _gatheringValueToday = value; OnPropertyChanged(); }
    }

    #endregion

    #region This Week

    public double FameThisWeek
    {
        get => _fameThisWeek;
        set { _fameThisWeek = value; OnPropertyChanged(); }
    }

    public double SilverThisWeek
    {
        get => _silverThisWeek;
        set { _silverThisWeek = value; OnPropertyChanged(); }
    }

    public double ReSpecThisWeek
    {
        get => _reSpecThisWeek;
        set { _reSpecThisWeek = value; OnPropertyChanged(); }
    }

    public double MightThisWeek
    {
        get => _mightThisWeek;
        set { _mightThisWeek = value; OnPropertyChanged(); }
    }

    public double FavorThisWeek
    {
        get => _favorThisWeek;
        set { _favorThisWeek = value; OnPropertyChanged(); }
    }

    public double FactionPointsThisWeek
    {
        get => _factionPointsThisWeek;
        set { _factionPointsThisWeek = value; OnPropertyChanged(); }
    }

    public double GatheringValueThisWeek
    {
        get => _gatheringValueThisWeek;
        set { _gatheringValueThisWeek = value; OnPropertyChanged(); }
    }

    #endregion

    #region Last Week

    public double FameLastWeek
    {
        get => _fameLastWeek;
        set { _fameLastWeek = value; OnPropertyChanged(); }
    }

    public double SilverLastWeek
    {
        get => _silverLastWeek;
        set { _silverLastWeek = value; OnPropertyChanged(); }
    }

    public double ReSpecLastWeek
    {
        get => _reSpecLastWeek;
        set { _reSpecLastWeek = value; OnPropertyChanged(); }
    }

    public double MightLastWeek
    {
        get => _mightLastWeek;
        set { _mightLastWeek = value; OnPropertyChanged(); }
    }

    public double FavorLastWeek
    {
        get => _favorLastWeek;
        set { _favorLastWeek = value; OnPropertyChanged(); }
    }

    public double FactionPointsLastWeek
    {
        get => _factionPointsLastWeek;
        set { _factionPointsLastWeek = value; OnPropertyChanged(); }
    }

    public double GatheringValueLastWeek
    {
        get => _gatheringValueLastWeek;
        set { _gatheringValueLastWeek = value; OnPropertyChanged(); }
    }

    #endregion

    #region This Month

    public double FameThisMonth
    {
        get => _fameThisMonth;
        set { _fameThisMonth = value; OnPropertyChanged(); }
    }

    public double SilverThisMonth
    {
        get => _silverThisMonth;
        set { _silverThisMonth = value; OnPropertyChanged(); }
    }

    public double ReSpecThisMonth
    {
        get => _reSpecThisMonth;
        set { _reSpecThisMonth = value; OnPropertyChanged(); }
    }

    public double MightThisMonth
    {
        get => _mightThisMonth;
        set { _mightThisMonth = value; OnPropertyChanged(); }
    }

    public double FavorThisMonth
    {
        get => _favorThisMonth;
        set { _favorThisMonth = value; OnPropertyChanged(); }
    }

    public double FactionPointsThisMonth
    {
        get => _factionPointsThisMonth;
        set { _factionPointsThisMonth = value; OnPropertyChanged(); }
    }

    public double GatheringValueThisMonth
    {
        get => _gatheringValueThisMonth;
        set { _gatheringValueThisMonth = value; OnPropertyChanged(); }
    }

    #endregion

    #region Last Month

    public double FameLastMonth
    {
        get => _fameLastMonth;
        set { _fameLastMonth = value; OnPropertyChanged(); }
    }

    public double SilverLastMonth
    {
        get => _silverLastMonth;
        set { _silverLastMonth = value; OnPropertyChanged(); }
    }

    public double ReSpecLastMonth
    {
        get => _reSpecLastMonth;
        set { _reSpecLastMonth = value; OnPropertyChanged(); }
    }

    public double MightLastMonth
    {
        get => _mightLastMonth;
        set { _mightLastMonth = value; OnPropertyChanged(); }
    }

    public double FavorLastMonth
    {
        get => _favorLastMonth;
        set { _favorLastMonth = value; OnPropertyChanged(); }
    }

    public double FactionPointsLastMonth
    {
        get => _factionPointsLastMonth;
        set { _factionPointsLastMonth = value; OnPropertyChanged(); }
    }

    public double GatheringValueLastMonth
    {
        get => _gatheringValueLastMonth;
        set { _gatheringValueLastMonth = value; OnPropertyChanged(); }
    }

    #endregion

    #region This Year

    public double FameThisYear
    {
        get => _fameThisYear;
        set { _fameThisYear = value; OnPropertyChanged(); }
    }

    public double SilverThisYear
    {
        get => _silverThisYear;
        set { _silverThisYear = value; OnPropertyChanged(); }
    }

    public double ReSpecThisYear
    {
        get => _reSpecThisYear;
        set { _reSpecThisYear = value; OnPropertyChanged(); }
    }

    public double MightThisYear
    {
        get => _mightThisYear;
        set { _mightThisYear = value; OnPropertyChanged(); }
    }

    public double FavorThisYear
    {
        get => _favorThisYear;
        set { _favorThisYear = value; OnPropertyChanged(); }
    }

    public double FactionPointsThisYear
    {
        get => _factionPointsThisYear;
        set { _factionPointsThisYear = value; OnPropertyChanged(); }
    }

    public double GatheringValueThisYear
    {
        get => _gatheringValueThisYear;
        set { _gatheringValueThisYear = value; OnPropertyChanged(); }
    }

    #endregion

    #region Total

    public double FameTotal
    {
        get => _fameTotal;
        set { _fameTotal = value; OnPropertyChanged(); }
    }

    public double SilverTotal
    {
        get => _silverTotal;
        set { _silverTotal = value; OnPropertyChanged(); }
    }

    public double ReSpecTotal
    {
        get => _reSpecTotal;
        set { _reSpecTotal = value; OnPropertyChanged(); }
    }

    public double MightTotal
    {
        get => _mightTotal;
        set { _mightTotal = value; OnPropertyChanged(); }
    }

    public double FavorTotal
    {
        get => _favorTotal;
        set { _favorTotal = value; OnPropertyChanged(); }
    }

    public double FactionPointsTotal
    {
        get => _factionPointsTotal;
        set { _factionPointsTotal = value; OnPropertyChanged(); }
    }

    public double GatheringValueTotal
    {
        get => _gatheringValueTotal;
        set { _gatheringValueTotal = value; OnPropertyChanged(); }
    }

    #endregion

    #region Translations – row labels

    public static string TranslationToday => LocalizationController.Translation("TODAY");
    public static string TranslationThisWeek => LocalizationController.Translation("THIS_WEEK");
    public static string TranslationLastWeek => LocalizationController.Translation("LAST_WEEK");
    public static string TranslationThisMonth => LocalizationController.Translation("MONTH");
    public static string TranslationLastMonth => LocalizationController.Translation("LAST_MONTH");
    public static string TranslationThisYear => LocalizationController.Translation("YEAR");
    public static string TranslationTotal => LocalizationController.Translation("TOTAL");

    #endregion

    #region Translations – column headers

    public static string TranslationFame => LocalizationController.Translation("FAME");
    public static string TranslationSilver => LocalizationController.Translation("SILVER");
    public static string TranslationReSpec => LocalizationController.Translation("RESPEC");
    public static string TranslationMight => LocalizationController.Translation("MIGHT");
    public static string TranslationFavor => LocalizationController.Translation("FAVOR");
    public static string TranslationFaction => LocalizationController.Translation("FACTION");

    #endregion

    #region Backing fields — Today per hour

    private double _fameTodayPerHour;
    private double _silverTodayPerHour;
    private double _reSpecTodayPerHour;
    private double _mightTodayPerHour;
    private double _favorTodayPerHour;
    private double _factionPointsTodayPerHour;
    private double _gatheringValueTodayPerHour;

    #endregion

    #region Backing fields — This Week per hour

    private double _fameThisWeekPerHour;
    private double _silverThisWeekPerHour;
    private double _reSpecThisWeekPerHour;
    private double _mightThisWeekPerHour;
    private double _favorThisWeekPerHour;
    private double _factionPointsThisWeekPerHour;
    private double _gatheringValueThisWeekPerHour;

    #endregion

    #region Backing fields — Last Week per hour

    private double _fameLastWeekPerHour;
    private double _silverLastWeekPerHour;
    private double _reSpecLastWeekPerHour;
    private double _mightLastWeekPerHour;
    private double _favorLastWeekPerHour;
    private double _factionPointsLastWeekPerHour;
    private double _gatheringValueLastWeekPerHour;

    #endregion

    #region Backing fields — This Month per hour

    private double _fameThisMonthPerHour;
    private double _silverThisMonthPerHour;
    private double _reSpecThisMonthPerHour;
    private double _mightThisMonthPerHour;
    private double _favorThisMonthPerHour;
    private double _factionPointsThisMonthPerHour;
    private double _gatheringValueThisMonthPerHour;

    #endregion

    #region Backing fields — Last Month per hour

    private double _fameLastMonthPerHour;
    private double _silverLastMonthPerHour;
    private double _reSpecLastMonthPerHour;
    private double _mightLastMonthPerHour;
    private double _favorLastMonthPerHour;
    private double _factionPointsLastMonthPerHour;
    private double _gatheringValueLastMonthPerHour;

    #endregion

    #region Backing fields — This Year per hour

    private double _fameThisYearPerHour;
    private double _silverThisYearPerHour;
    private double _reSpecThisYearPerHour;
    private double _mightThisYearPerHour;
    private double _favorThisYearPerHour;
    private double _factionPointsThisYearPerHour;
    private double _gatheringValueThisYearPerHour;

    #endregion

    #region Backing fields — Total per hour

    private double _fameTotalPerHour;
    private double _silverTotalPerHour;
    private double _reSpecTotalPerHour;
    private double _mightTotalPerHour;
    private double _favorTotalPerHour;
    private double _factionPointsTotalPerHour;
    private double _gatheringValueTotalPerHour;

    #endregion

    #region Backing fields — Session

    private double _fameSession;
    private double _silverSession;
    private double _reSpecSession;
    private double _mightSession;
    private double _favorSession;
    private double _factionPointsSession;
    private double _fameSessionPerHour;
    private double _silverSessionPerHour;
    private double _reSpecSessionPerHour;
    private double _mightSessionPerHour;
    private double _favorSessionPerHour;
    private double _factionPointsSessionPerHour;
    private double _gatheringValueSession;
    private double _gatheringValueSessionPerHour;
    private CityFaction _cityFaction;
    private string _activeTimeSession = string.Empty;
    private string _activeTimeToday = string.Empty;
    private string _activeTimeThisWeek = string.Empty;
    private string _activeTimeLastWeek = string.Empty;
    private string _activeTimeThisMonth = string.Empty;
    private string _activeTimeLastMonth = string.Empty;
    private string _activeTimeThisYear = string.Empty;
    private string _activeTimeTotal = string.Empty;

    #endregion

    #region Today per hour

    public double FameTodayPerHour { get => _fameTodayPerHour; set { _fameTodayPerHour = value; OnPropertyChanged(); } }
    public double SilverTodayPerHour { get => _silverTodayPerHour; set { _silverTodayPerHour = value; OnPropertyChanged(); } }
    public double ReSpecTodayPerHour { get => _reSpecTodayPerHour; set { _reSpecTodayPerHour = value; OnPropertyChanged(); } }
    public double MightTodayPerHour { get => _mightTodayPerHour; set { _mightTodayPerHour = value; OnPropertyChanged(); } }
    public double FavorTodayPerHour { get => _favorTodayPerHour; set { _favorTodayPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsTodayPerHour { get => _factionPointsTodayPerHour; set { _factionPointsTodayPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueTodayPerHour { get => _gatheringValueTodayPerHour; set { _gatheringValueTodayPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region This Week per hour

    public double FameThisWeekPerHour { get => _fameThisWeekPerHour; set { _fameThisWeekPerHour = value; OnPropertyChanged(); } }
    public double SilverThisWeekPerHour { get => _silverThisWeekPerHour; set { _silverThisWeekPerHour = value; OnPropertyChanged(); } }
    public double ReSpecThisWeekPerHour { get => _reSpecThisWeekPerHour; set { _reSpecThisWeekPerHour = value; OnPropertyChanged(); } }
    public double MightThisWeekPerHour { get => _mightThisWeekPerHour; set { _mightThisWeekPerHour = value; OnPropertyChanged(); } }
    public double FavorThisWeekPerHour { get => _favorThisWeekPerHour; set { _favorThisWeekPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsThisWeekPerHour { get => _factionPointsThisWeekPerHour; set { _factionPointsThisWeekPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueThisWeekPerHour { get => _gatheringValueThisWeekPerHour; set { _gatheringValueThisWeekPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region Last Week per hour

    public double FameLastWeekPerHour { get => _fameLastWeekPerHour; set { _fameLastWeekPerHour = value; OnPropertyChanged(); } }
    public double SilverLastWeekPerHour { get => _silverLastWeekPerHour; set { _silverLastWeekPerHour = value; OnPropertyChanged(); } }
    public double ReSpecLastWeekPerHour { get => _reSpecLastWeekPerHour; set { _reSpecLastWeekPerHour = value; OnPropertyChanged(); } }
    public double MightLastWeekPerHour { get => _mightLastWeekPerHour; set { _mightLastWeekPerHour = value; OnPropertyChanged(); } }
    public double FavorLastWeekPerHour { get => _favorLastWeekPerHour; set { _favorLastWeekPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsLastWeekPerHour { get => _factionPointsLastWeekPerHour; set { _factionPointsLastWeekPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueLastWeekPerHour { get => _gatheringValueLastWeekPerHour; set { _gatheringValueLastWeekPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region This Month per hour

    public double FameThisMonthPerHour { get => _fameThisMonthPerHour; set { _fameThisMonthPerHour = value; OnPropertyChanged(); } }
    public double SilverThisMonthPerHour { get => _silverThisMonthPerHour; set { _silverThisMonthPerHour = value; OnPropertyChanged(); } }
    public double ReSpecThisMonthPerHour { get => _reSpecThisMonthPerHour; set { _reSpecThisMonthPerHour = value; OnPropertyChanged(); } }
    public double MightThisMonthPerHour { get => _mightThisMonthPerHour; set { _mightThisMonthPerHour = value; OnPropertyChanged(); } }
    public double FavorThisMonthPerHour { get => _favorThisMonthPerHour; set { _favorThisMonthPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsThisMonthPerHour { get => _factionPointsThisMonthPerHour; set { _factionPointsThisMonthPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueThisMonthPerHour { get => _gatheringValueThisMonthPerHour; set { _gatheringValueThisMonthPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region Last Month per hour

    public double FameLastMonthPerHour { get => _fameLastMonthPerHour; set { _fameLastMonthPerHour = value; OnPropertyChanged(); } }
    public double SilverLastMonthPerHour { get => _silverLastMonthPerHour; set { _silverLastMonthPerHour = value; OnPropertyChanged(); } }
    public double ReSpecLastMonthPerHour { get => _reSpecLastMonthPerHour; set { _reSpecLastMonthPerHour = value; OnPropertyChanged(); } }
    public double MightLastMonthPerHour { get => _mightLastMonthPerHour; set { _mightLastMonthPerHour = value; OnPropertyChanged(); } }
    public double FavorLastMonthPerHour { get => _favorLastMonthPerHour; set { _favorLastMonthPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsLastMonthPerHour { get => _factionPointsLastMonthPerHour; set { _factionPointsLastMonthPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueLastMonthPerHour { get => _gatheringValueLastMonthPerHour; set { _gatheringValueLastMonthPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region This Year per hour

    public double FameThisYearPerHour { get => _fameThisYearPerHour; set { _fameThisYearPerHour = value; OnPropertyChanged(); } }
    public double SilverThisYearPerHour { get => _silverThisYearPerHour; set { _silverThisYearPerHour = value; OnPropertyChanged(); } }
    public double ReSpecThisYearPerHour { get => _reSpecThisYearPerHour; set { _reSpecThisYearPerHour = value; OnPropertyChanged(); } }
    public double MightThisYearPerHour { get => _mightThisYearPerHour; set { _mightThisYearPerHour = value; OnPropertyChanged(); } }
    public double FavorThisYearPerHour { get => _favorThisYearPerHour; set { _favorThisYearPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsThisYearPerHour { get => _factionPointsThisYearPerHour; set { _factionPointsThisYearPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueThisYearPerHour { get => _gatheringValueThisYearPerHour; set { _gatheringValueThisYearPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region Total per hour

    public double FameTotalPerHour { get => _fameTotalPerHour; set { _fameTotalPerHour = value; OnPropertyChanged(); } }
    public double SilverTotalPerHour { get => _silverTotalPerHour; set { _silverTotalPerHour = value; OnPropertyChanged(); } }
    public double ReSpecTotalPerHour { get => _reSpecTotalPerHour; set { _reSpecTotalPerHour = value; OnPropertyChanged(); } }
    public double MightTotalPerHour { get => _mightTotalPerHour; set { _mightTotalPerHour = value; OnPropertyChanged(); } }
    public double FavorTotalPerHour { get => _favorTotalPerHour; set { _favorTotalPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsTotalPerHour { get => _factionPointsTotalPerHour; set { _factionPointsTotalPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueTotalPerHour { get => _gatheringValueTotalPerHour; set { _gatheringValueTotalPerHour = value; OnPropertyChanged(); } }

    #endregion

    #region Session

    public double FameSession { get => _fameSession; set { _fameSession = value; OnPropertyChanged(); } }
    public double SilverSession { get => _silverSession; set { _silverSession = value; OnPropertyChanged(); } }
    public double ReSpecSession { get => _reSpecSession; set { _reSpecSession = value; OnPropertyChanged(); } }
    public double MightSession { get => _mightSession; set { _mightSession = value; OnPropertyChanged(); } }
    public double FavorSession { get => _favorSession; set { _favorSession = value; OnPropertyChanged(); } }
    public double FactionPointsSession { get => _factionPointsSession; set { _factionPointsSession = value; OnPropertyChanged(); } }
    public double FameSessionPerHour { get => _fameSessionPerHour; set { _fameSessionPerHour = value; OnPropertyChanged(); } }
    public double SilverSessionPerHour { get => _silverSessionPerHour; set { _silverSessionPerHour = value; OnPropertyChanged(); } }
    public double ReSpecSessionPerHour { get => _reSpecSessionPerHour; set { _reSpecSessionPerHour = value; OnPropertyChanged(); } }
    public double MightSessionPerHour { get => _mightSessionPerHour; set { _mightSessionPerHour = value; OnPropertyChanged(); } }
    public double FavorSessionPerHour { get => _favorSessionPerHour; set { _favorSessionPerHour = value; OnPropertyChanged(); } }
    public double FactionPointsSessionPerHour { get => _factionPointsSessionPerHour; set { _factionPointsSessionPerHour = value; OnPropertyChanged(); } }
    public double GatheringValueSession { get => _gatheringValueSession; set { _gatheringValueSession = value; OnPropertyChanged(); } }
    public double GatheringValueSessionPerHour { get => _gatheringValueSessionPerHour; set { _gatheringValueSessionPerHour = value; OnPropertyChanged(); } }
    public CityFaction CityFaction { get => _cityFaction; set { _cityFaction = value; OnPropertyChanged(); } }
    public string ActiveTimeSession { get => _activeTimeSession; set { _activeTimeSession = value; OnPropertyChanged(); } }
    public string ActiveTimeToday { get => _activeTimeToday; set { _activeTimeToday = value; OnPropertyChanged(); } }
    public string ActiveTimeThisWeek { get => _activeTimeThisWeek; set { _activeTimeThisWeek = value; OnPropertyChanged(); } }
    public string ActiveTimeLastWeek { get => _activeTimeLastWeek; set { _activeTimeLastWeek = value; OnPropertyChanged(); } }
    public string ActiveTimeThisMonth { get => _activeTimeThisMonth; set { _activeTimeThisMonth = value; OnPropertyChanged(); } }
    public string ActiveTimeLastMonth { get => _activeTimeLastMonth; set { _activeTimeLastMonth = value; OnPropertyChanged(); } }
    public string ActiveTimeThisYear { get => _activeTimeThisYear; set { _activeTimeThisYear = value; OnPropertyChanged(); } }
    public string ActiveTimeTotal { get => _activeTimeTotal; set { _activeTimeTotal = value; OnPropertyChanged(); } }

    #endregion

    public static string TranslationSession => LocalizationController.Translation("SESSION");
    public static string TranslationGatheringValue => LocalizationController.Translation("GATHERING");
}
