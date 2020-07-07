using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class SettingsWindowTranslation
    {
        public string Settings => LanguageController.Translation("SETTINGS");
        public string Language => LanguageController.Translation("LANGUAGE");
        public string RefreshRate => LanguageController.Translation("REFRESH_RATE");
        public string UpdateItemListByDays => LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS");
        public string ItemListSourceUrl => LanguageController.Translation("ITEM_LIST_SOURCE_URL");
        public string OpenItemWindowInNewWindow => LanguageController.Translation("OPEN_ITEM_WINDOW_IN_NEW_WINDOW");
        public string ShowInfoWindowOnStart => LanguageController.Translation("SHOW_INFO_WINDOW_ON_START");
        public string Save => LanguageController.Translation("SAVE");
        public string FullItemInformationUpdateCycleDays => LanguageController.Translation("FULL_ITEM_INFO_UPDATE_CYCLE_DAYS");
    }
}