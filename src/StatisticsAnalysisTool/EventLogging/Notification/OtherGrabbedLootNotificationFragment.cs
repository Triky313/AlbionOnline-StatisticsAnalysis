using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class OtherGrabbedLootNotificationFragment : LineFragment
{
    public OtherGrabbedLootNotificationFragment(string lootedByName, string lootedFromName, string lootedByGuild, string lootedFromGuild, Item item, int quantity)
    {
        LootedByName = lootedByName;
        LootedByGuild = lootedByGuild;
        LootedFromName = lootedFromName;
        LootedFromGuild = lootedFromGuild;
        LocalizedName = item.LocalizedName;
        Icon = item.Icon;
        Quantity = quantity;
        AverageEstMarketValue = item.AverageEstMarketValue;
        AverageEstMarketValueShortString = AverageEstMarketValue.ToShortNumberString();
        EstimatedMarketValueDisplayString = GetEstimatedMarketValueDisplayString(quantity, AverageEstMarketValue, AverageEstMarketValueShortString);
    }

    public string LootedByName { get; }
    public string LootedByGuild { get; }
    public bool IsLootedByGuildEmpty => string.IsNullOrEmpty(LootedByGuild);
    public string LocalizedName { get; }
    public BitmapImage Icon { get; }
    public int Quantity { get; }
    public long AverageEstMarketValue { get; set; }
    public string AverageEstMarketValueShortString { get; }
    public string EstimatedMarketValueDisplayString { get; }
    public string LootedFromName { get; }
    public string LootedFromGuild { get; }
    public bool IsLootedFromGuildEmpty => string.IsNullOrEmpty(LootedFromGuild);
    public bool IsLootedPlayerMob => LootedFromName.ToUpper().Equals("MOB");

    public static string FromTranslation => LocalizationController.Translation("FROM");
    public static string LootedTranslation => LocalizationController.Translation("LOOTED");
    public static string TranslationGuild => LocalizationController.Translation("GUILD_CAP");
    public static string TranslationAverageEstMarketValue => LocalizationController.Translation("AVERAGE_EST_MARKET_VALUE");

    private static string GetEstimatedMarketValueDisplayString(int quantity, long averageEstMarketValue, string averageEstMarketValueShortString)
    {
        if (quantity <= 1)
        {
            return averageEstMarketValueShortString;
        }

        return LocalizationController.Translation("LOOT_NOTIFICATION_UNIT_PRICE_FORMAT",
            ["TOTAL_VALUE", "UNIT_PRICE"],
            [((double) averageEstMarketValue * quantity).ToShortNumberString(), averageEstMarketValueShortString]);
    }
}
