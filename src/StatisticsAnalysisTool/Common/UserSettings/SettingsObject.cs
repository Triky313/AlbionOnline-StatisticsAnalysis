namespace StatisticsAnalysisTool.Common.UserSettings
{
    public class SettingsObject
    {
        public string CurrentLanguageCultureName { get; set; } = "en-US";
        public int RefreshRate { get; set; } = 10000;
        public string MainTrackingCharacterName { get; set; }
        public int UpdateItemListByDays { get; set; } = 7;
        public int UpdateItemsJsonByDays { get; set; } = 7;
        public string ItemListSourceUrl { get; set; } = "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json";
        public string ItemsJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/items.json";
        public bool IsOpenItemWindowInNewWindowChecked { get; set; } = true;
        public bool IsInfoWindowShownOnStart { get; set; } = true;
        public string SelectedAlertSound { get; set; }
        public string CityPricesApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/prices/";
        public string CityPricesHistoryApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/history/";
        public string GoldStatsApiUrl { get; set; } = "https://www.albion-online-data.com/api/v2/stats/Gold";
        public double MainWindowHeight { get; set; } = 100;
        public double MainWindowWidth { get; set; } = 100;
        public bool MainWindowMaximized { get; set; }
        public bool IsTrackingResetByMapChangeActive { get; set; }
        public bool IsMainTrackerFilterSilver { get; set; }
        public bool IsMainTrackerFilterFame { get; set; }
        public bool IsMainTrackerFilterFaction { get; set; }
        public bool IsMainTrackerFilterSeasonPoints { get; set; }
        public bool IsMainTrackerFilterEquipmentLoot { get; set; }
        public bool IsMainTrackerFilterConsumableLoot { get; set; }
        public bool IsMainTrackerFilterSimpleLoot { get; set; }
        public bool IsMainTrackerFilterUnknownLoot { get; set; }
        public bool IsMainTrackerFilterKill { get; set; }
        public bool IsDamageMeterTrackingActive { get; set; } = true;
        public bool IsTrackingPartyLootOnly { get; set; }
        public bool IsTrackingSilver { get; set; }
        public bool IsTrackingFame { get; set; }
        public bool IsTrackingMobLoot { get; set; }
        public string SavedPlayerInformationName { get; set; }
        public bool IsLootLoggerSaveReminderActive { get; set; }
        public bool IsSuggestPreReleaseUpdatesActive { get; set; }
        public bool IsLootFromMobShown { get; set; }
        public double MailMonitoringGridSplitterPosition { get; set; } = 125;
        public double DungeonsGridSplitterPosition { get; set; } = 125;
        public double StorageHistoryGridSplitterPosition { get; set; } = 125;
        public double DamageMeterGridSplitterPosition { get; set; } = 125;
        public bool ShortDamageMeterToClipboard { get; set; }
        public bool IsMailMonitoringActive { get; set; } = true;
        public bool IgnoreMailsWithZeroValues { get; set; } = false;
        public int DeleteMailsOlderThanSpecifiedDays { get; set; } = 0;
        public bool IsSnapshotAfterMapChangeActive { get; set; } = false;
        public bool IsDamageMeterResetByMapChangeActive { get; set; } = false;
    }
}