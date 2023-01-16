using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.TranslationModel
{
    public class SettingsWindowTranslation
    {
        public static string Settings => LanguageController.Translation("SETTINGS");
        public static string Language => $"{LanguageController.Translation("LANGUAGE")} ({LanguageController.Translation("PROGRAM_RESTART_REQUIRED")})";
        public static string CheckForUpdate => LanguageController.Translation("CHECK_FOR_UPDATE");
        public static string RefreshRate => LanguageController.Translation("REFRESH_RATE");
        public static string UpdateItemListByDays => LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS");
        public static string UpdateItemsJsonByDays => LanguageController.Translation("UPDATE_ITEMS_JSON_BY_DAYS");
        public static string UpdateMobsJsonByDays => LanguageController.Translation("UPDATE_MOBS_JSON_BY_DAYS");
        public static string ItemListSourceUrl => LanguageController.Translation("ITEM_LIST_SOURCE_URL");
        public static string ItemsJsonSourceUrl => LanguageController.Translation("ITEMS_JSON_SOURCE_URL");
        public static string MobsJsonSourceUrl => LanguageController.Translation("MOBS_JSON_SOURCE_URL");
        public static string OpenItemWindowInNewWindow => LanguageController.Translation("OPEN_ITEM_WINDOW_IN_NEW_WINDOW");
        public static string ShowInfoWindowOnStart => LanguageController.Translation("SHOW_INFO_WINDOW_ON_START");
        public static string Save => LanguageController.Translation("SAVE");
        public static string AlarmSoundUsed => LanguageController.Translation("ALARM_SOUND_USED");
        public static string ToolDirectory => LanguageController.Translation("TOOL_DIRECTORY");
        public static string OpenToolDirectory => LanguageController.Translation("OPEN_TOOL_DIRECTORY");
        public static string OpenDebugConsole => LanguageController.Translation("OPEN_DEBUG_CONSOLE");
        public static string CreateDesktopShortcut => LanguageController.Translation("CREATE_DESKTOP_SHORTCUT");
        public static string CityPricesApiUrl => LanguageController.Translation("CITY_PRICES_API_URL");
        public static string CityPricesHistoryApiUrl => LanguageController.Translation("CITY_PRICES_HISTORY_API_URL");
        public static string GoldStatsApiUrl => LanguageController.Translation("GOLD_STATS_API_URL");
        public static string IsLootLoggerSaveReminderActive => LanguageController.Translation("IS_LOOT_LOGGER_SAVE_REMINDER_ACTIVE");
        public static string ExportLootLoggingFileWithRealItemName => LanguageController.Translation("EXPORT_LOOT_LOGGING_FILE_WITH_REAL_ITEM_NAME");
        public static string FiveSeconds => LanguageController.Translation("5_SECONDS");
        public static string TenSeconds => LanguageController.Translation("10_SECONDS");
        public static string ThirtySeconds => LanguageController.Translation("30_SECONDS");
        public static string SixtySeconds => LanguageController.Translation("60_SECONDS");
        public static string FiveMinutes => LanguageController.Translation("5_MINUTES");
        public static string SuggestPreReleaseUpdates => LanguageController.Translation("SUGGEST_PRE_RELEASE_UPDATES");
        public static string AttentionTheseVersionsAreStillBeingTested => LanguageController.Translation("ATTENTION_THESE_VERSION_ARE_STILL_BEING_TESTED");
        public static string CharacterNameToTrack => LanguageController.Translation("CHARACTER_NAME_TO_TRACK");
        public static string CopyShortDamageMeterToTheClipboard => LanguageController.Translation("COPY_SHORT_DAMAGE_METER_TO_THE_CLIPBOARD");
    }
}