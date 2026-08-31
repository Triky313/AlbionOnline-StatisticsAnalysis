using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class OtherGrabbedLootNotificationFragment : LineFragment
{
    public OtherGrabbedLootNotificationFragment(string lootedByName, string lootedFromName, string lootedByGuild, string lootedByAlliance,
        string lootedFromGuild, string lootedFromAlliance, Item item, int quantity)
    {
        LootedByName = lootedByName;
        LootedByGuild = lootedByGuild;
        LootedByAlliance = lootedByAlliance;
        LootedByAffiliations = BuildAffiliations(lootedByGuild, lootedByAlliance);
        LootedFromName = lootedFromName;
        LootedFromGuild = lootedFromGuild;
        LootedFromAlliance = lootedFromAlliance;
        LootedFromAffiliations = BuildAffiliations(lootedFromGuild, lootedFromAlliance);
        LocalizedName = item.LocalizedName;
        Icon = item.Icon;
        Quantity = quantity;
        AverageEstMarketValue = item.AverageEstMarketValue;
        AverageEstMarketValueShortString = AverageEstMarketValue.ToShortNumberString();
        EstimatedMarketValueDisplayString = GetEstimatedMarketValueDisplayString(quantity, AverageEstMarketValue, AverageEstMarketValueShortString);
    }

    public string LootedByName { get; }
    public string LootedByGuild { get; }
    public string LootedByAlliance { get; }
    public string LootedByAffiliations { get; }
    public bool IsLootedByAffiliationsEmpty => string.IsNullOrEmpty(LootedByAffiliations);
    public string LocalizedName { get; }
    public BitmapImage Icon { get; }
    public int Quantity { get; }
    public long AverageEstMarketValue { get; set; }
    public string AverageEstMarketValueShortString { get; }
    public string EstimatedMarketValueDisplayString { get; }
    public string LootedFromName { get; }
    public string LootedFromGuild { get; }
    public string LootedFromAlliance { get; }
    public string LootedFromAffiliations { get; }
    public bool IsLootedFromAffiliationsEmpty => string.IsNullOrEmpty(LootedFromAffiliations);
    public bool IsLootedPlayerMob => LootedFromName.ToUpper().Equals("MOB");

    public static string FromTranslation => LocalizationController.Translation("FROM");
    public static string LootedTranslation => LocalizationController.Translation("LOOTED");
    public static string TranslationGuild => LocalizationController.Translation("GUILD_CAP");
    public static string TranslationAlliance => LocalizationController.Translation("ALLIANCE");
    public static string TranslationAffiliations => $"{TranslationGuild} / {TranslationAlliance}";
    public static string TranslationAverageEstMarketValue => LocalizationController.Translation("AVERAGE_EST_MARKET_VALUE");

    private static string BuildAffiliations(string guild, string alliance)
    {
        var normalizedGuild = guild?.Trim() ?? string.Empty;
        var normalizedAlliance = alliance?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedGuild))
        {
            return normalizedAlliance;
        }

        return string.IsNullOrEmpty(normalizedAlliance)
            ? normalizedGuild
            : $"{normalizedGuild}, {normalizedAlliance}";
    }

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
