using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.TranslationModel
{
    public class DashboardWindowTranslation
    {
        public string Title => $"{LanguageController.Translation("DASHBOARD")}";
        public static string Fame => LanguageController.Translation("FAME");
        public static string Silver => LanguageController.Translation("SILVER");
        public static string ReSpec => LanguageController.Translation("RESPEC");
        public static string Faction => LanguageController.Translation("FACTION");
        public static string Might => LanguageController.Translation("MIGHT");
        public static string Favor => LanguageController.Translation("FAVOR");
        public static string ResetTrackingCounter => LanguageController.Translation("RESET_TRACKING_COUNTER");
        public static string Today => LanguageController.Translation("TODAY").ToLower();
        public static string Week => LanguageController.Translation("WEEK").ToLower();
        public static string Month => LanguageController.Translation("MONTH").ToLower();
        public static string Kills => LanguageController.Translation("KILLS");
        public static string SoloKills => LanguageController.Translation("SOLO_KILLS");
        public static string Deaths => LanguageController.Translation("DEATHS");
        public static string LastUpdate => LanguageController.Translation("LAST_UPDATE");
        public static string DataFromAlbionOnlineServers => LanguageController.Translation("DATA_FROM_ALBION_ONLINE_SERVERS");
        public static string AverageItemPowerWhenKilling => LanguageController.Translation("AVERAGE_ITEM_POWER_WHEN_KILLING");
        public static string AverageItemPowerOfTheKilledEnemies => LanguageController.Translation("AVERAGE_ITEM_POWER_OF_THE_KILLED_ENEMIES");
        public static string AverageItemPowerWhenDying => LanguageController.Translation("AVERAGE_ITEM_POWER_WHEN_DYING");
    }
}