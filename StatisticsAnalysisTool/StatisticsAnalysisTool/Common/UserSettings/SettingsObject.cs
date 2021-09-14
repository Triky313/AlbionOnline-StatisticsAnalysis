namespace StatisticsAnalysisTool.Common.UserSettings
{
    public class SettingsObject
    {
        public string CurrentLanguageCultureName { get; set; }
        public int RefreshRate { get; set; }
        public int UpdateItemListByDays { get; set; }
        public string ItemListSourceUrl { get; set; }
        public bool IsOpenItemWindowInNewWindowChecked { get; set; }
        public bool IsInfoWindowShownOnStart { get; set; }
        public int FullItemInformationUpdateCycleDays { get; set; }
        public string SelectedAlertSound { get; set; }
        public bool CityPricesApiUrl { get; set; }
        public int CityPricesHistoryApiUrl { get; set; }
        public string GoldStatsApiUrl { get; set; }
    }
}