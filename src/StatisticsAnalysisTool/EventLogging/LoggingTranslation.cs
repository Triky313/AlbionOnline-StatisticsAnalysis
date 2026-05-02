using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingTranslation
{
    public static string Lost => LocalizationController.Translation("LOST");
    public static string Resolved => LocalizationController.Translation("RESOLVED");
    public static string Donated => LocalizationController.Translation("DONATED");
    public static string Trash => LocalizationController.Translation("TRASH");
    public static string T1ToT3 => LocalizationController.Translation("T1_TO_T3");
    public static string T4 => LocalizationController.Translation("T4");
    public static string T5 => LocalizationController.Translation("T5");
    public static string T6 => LocalizationController.Translation("T6");
    public static string T7 => LocalizationController.Translation("T7");
    public static string T8 => LocalizationController.Translation("T8");
    public static string Bag => LocalizationController.Translation("BAG");
    public static string Cape => LocalizationController.Translation("CAPE");
    public static string Food => LocalizationController.Translation("FOOD");
    public static string Potion => LocalizationController.Translation("POTION");
    public static string Mount => LocalizationController.Translation("MOUNT");
    public static string Others => LocalizationController.Translation("OTHERS");
    public static string FilterStatus => LocalizationController.Translation("LOOT_FILTER_STATUS");
    public static string FilterTier => LocalizationController.Translation("LOOT_FILTER_TIER");
    public static string FilterType => LocalizationController.Translation("LOOT_FILTER_TYPE");
    public static string FilterAll => LocalizationController.Translation("LOOT_FILTER_ALL");
    public static string FilterNone => LocalizationController.Translation("LOOT_FILTER_NONE");
    public static string UploadChestFiles => LocalizationController.Translation("UPLOAD_CHEST_FILES");
    public static string AddLootLogFiles => LocalizationController.Translation("ADD_LOOT_LOG_FILES");
    public static string CompareLogs => LocalizationController.Translation("COMPARE_LOGS");
    public static string DeleteChestLogs => LocalizationController.Translation("DELETE_CHEST_LOGS");
    public static string DeleteAllLogs => LocalizationController.Translation("DELETE_ALL_LOGS");
}