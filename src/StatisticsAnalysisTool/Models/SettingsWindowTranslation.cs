using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class SettingsWindowTranslation
    {
        public string Settings => LanguageController.Translation("SETTINGS");
        public string Language => $"{LanguageController.Translation("LANGUAGE")} ({LanguageController.Translation("PROGRAM_RESTART_REQUIRED")})";
        public string RefreshRate => LanguageController.Translation("REFRESH_RATE");
        public string UpdateItemListByDays => LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS");
        public string ItemListSourceUrl => LanguageController.Translation("ITEM_LIST_SOURCE_URL");
        public string OpenItemWindowInNewWindow => LanguageController.Translation("OPEN_ITEM_WINDOW_IN_NEW_WINDOW");
        public string ShowInfoWindowOnStart => LanguageController.Translation("SHOW_INFO_WINDOW_ON_START");
        public string Save => LanguageController.Translation("SAVE");
        public string FullItemInformationUpdateCycleDays => LanguageController.Translation("FULL_ITEM_INFO_UPDATE_CYCLE_DAYS");
        public string AlarmSoundUsed => LanguageController.Translation("ALARM_SOUND_USED");
        public string ToolDirectory => LanguageController.Translation("TOOL_DIRECTORY");
        public string OpenToolDirectory => LanguageController.Translation("OPEN_TOOL_DIRECTORY");
        public string OpenCloseDebugConsole => LanguageController.Translation("OPEN_OR_CLOSE_DEBUG_CONSOLE");
        public string CreateDesktopShortcut => LanguageController.Translation("CREATE_DESKTOP_SHORTCUT");
        public string CityPricesApiUrl => LanguageController.Translation("CITY_PRICES_API_URL");
        public string CityPricesHistoryApiUrl => LanguageController.Translation("CITY_PRICES_HISTORY_API_URL");
        public string GoldStatsApiUrl => LanguageController.Translation("GOLD_STATS_API_URL");
        public string IsLootLoggerSaveReminderActive => LanguageController.Translation("IS_LOOT_LOGGER_SAVE_REMINDER_ACTIVE");
        public string FiveSeconds => LanguageController.Translation("5_SECONDS");
        public string TenSeconds => LanguageController.Translation("10_SECONDS");
        public string ThirtySeconds => LanguageController.Translation("30_SECONDS");
        public string SixtySeconds => LanguageController.Translation("60_SECONDS");
        public string FiveMinutes => LanguageController.Translation("5_MINUTES");
    }
}