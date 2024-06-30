using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class DonationTranslation
{
    public static string HowCanIDonate => LocalizationController.Translation("HOW_CAN_I_DONATE");
    public static string TopDonationsAllTime => LocalizationController.Translation("TOP_DONATIONS_ALL_TIME");
    public static string TopDonationsThisMonth => LocalizationController.Translation("TOP_DONATIONS_THIS_MONTH");
    public static string DonationDescription => LocalizationController.Translation("DONATION_DESCRIPTION");
    public static string NoDonationsYet => LocalizationController.Translation("NO_DONATION_YET");
    public static string WhyDonate => LocalizationController.Translation("WHY_DONATE");
    public static string WhyDonateDescription => LocalizationController.Translation("WHY_DONATE_DESCRIPTION");
    public static string TopRealMoneyDonations => LocalizationController.Translation("TOP_REAL_MONEY_DONATIONS");
    public static string ItemsAreConvertedToSilver => LocalizationController.Translation("ITEMS_ARE_CONVERTED_TO_SILVER");
}