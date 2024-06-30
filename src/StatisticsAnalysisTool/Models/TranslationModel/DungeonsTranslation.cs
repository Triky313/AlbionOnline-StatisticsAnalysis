using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class DungeonsTranslation
{
    public static string Total => LocalizationController.Translation("TOTAL");
    public static string DungeonStats => LocalizationController.Translation("DUNGEON_STATS");
    public static string ChestStats => LocalizationController.Translation("CHEST_STATS");
    public static string BookChestStats => LocalizationController.Translation("BOOK_CHEST_STATS");
    public static string EnteredDungeon => LocalizationController.Translation("ENTERED_DUNGEON");
    public static string OpenedStandardChests => LocalizationController.Translation("OPENED_STANDARD_CHESTS");
    public static string OpenedUncommonChests => LocalizationController.Translation("OPENED_UNCOMMON_CHESTS");
    public static string OpenedRareChests => LocalizationController.Translation("OPENED_RARE_CHESTS");
    public static string OpenedLegendaryChests => LocalizationController.Translation("OPENED_LEGENDARY_CHESTS");
    public static string AverageFame => LocalizationController.Translation("AVERAGE_FAME");
    public static string AverageReSpec => LocalizationController.Translation("AVERAGE_RESPEC");
    public static string AverageSilver => LocalizationController.Translation("AVERAGE_SILVER");
    public static string DeleteAndReset => LocalizationController.Translation("DELETE_AND_RESET");
    public static string BestLootedItem => LocalizationController.Translation("BEST_LOOTED_ITEM");
}