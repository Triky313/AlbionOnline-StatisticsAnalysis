using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class DungeonsTranslation
{
    public static string Total => LanguageController.Translation("TOTAL");
    public static string DungeonStats => LanguageController.Translation("DUNGEON_STATS");
    public static string ChestStats => LanguageController.Translation("CHEST_STATS");
    public static string BookChestStats => LanguageController.Translation("BOOK_CHEST_STATS");
    public static string EnteredDungeon => LanguageController.Translation("ENTERED_DUNGEON");
    public static string OpenedStandardChests => LanguageController.Translation("OPENED_STANDARD_CHESTS");
    public static string OpenedUncommonChests => LanguageController.Translation("OPENED_UNCOMMON_CHESTS");
    public static string OpenedRareChests => LanguageController.Translation("OPENED_RARE_CHESTS");
    public static string OpenedLegendaryChests => LanguageController.Translation("OPENED_LEGENDARY_CHESTS");
    public static string AverageFame => LanguageController.Translation("AVERAGE_FAME");
    public static string AverageReSpec => LanguageController.Translation("AVERAGE_RESPEC");
    public static string AverageSilver => LanguageController.Translation("AVERAGE_SILVER");
    public static string DeleteAndReset => LanguageController.Translation("DELETE_AND_RESET");
    public static string BestLootedItem => LanguageController.Translation("BEST_LOOTED_ITEM");
}