namespace StatisticsAnalysisTool.Common.UserSettings
{
    public class SettingsObject
    {
        public string CurrentLanguageCultureName { get; set; } = "en-US";
        public int RefreshRate { get; set; } = 10000;
        public int UpdateItemListByDays { get; set; } = 7;
        public string ItemListSourceUrl { get; set; } = "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json";
        public bool IsOpenItemWindowInNewWindowChecked { get; set; } = true;
        public bool IsInfoWindowShownOnStart { get; set; } = true;
        public int FullItemInformationUpdateCycleDays { get; set; } = 90;
        public string SelectedAlertSound { get; set; }
        public string CityPricesApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/prices/";
        public string CityPricesHistoryApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/history/";
        public string GoldStatsApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/Gold";
    }
}