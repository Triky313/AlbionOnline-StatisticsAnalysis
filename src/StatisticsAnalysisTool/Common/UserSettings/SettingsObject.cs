using System;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Models;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common.UserSettings;

public class SettingsObject
{
    public string CurrentLanguageCultureName { get; set; } = "en-US";
    public int RefreshRate { get; set; } = 10000;
    public int Server { get; set; } = 0; // 0: auto, 1: west, 2: east
    public string PacketFilter { get; set; } = "(host 5.45.187 or host 5.188.125) and udp port 5056";
    public string MainTrackingCharacterName { get; set; }
    public int UpdateItemListByDays { get; set; } = 7;
    public int UpdateItemsJsonByDays { get; set; } = 7;
    public int UpdateMobsJsonByDays { get; set; } = 7;
    public int UpdateSpellsJsonByDays { get; set; } = 7;
    public int UpdateLootChestJsonByDays { get; set; } = 7;
    public int UpdateWorldJsonByDays { get; set; } = 7;
    public string ItemListSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/formatted/items.json";
    public string ItemsJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/items.json";
    public string MobsJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/mobs.json";
    public string SpellsJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/spells.json";
    public string LootChestJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/lootchests.json";
    public string WorldJsonSourceUrl { get; set; } = "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/cluster/world.json";
    public bool IsOpenItemWindowInNewWindowChecked { get; set; } = true;
    public bool IsInfoWindowShownOnStart { get; set; } = true;
    public string SelectedAlertSound { get; set; }
    public string AlbionDataProjectBaseUrlWest { get; set; } = "https://albion-online-data.com/api/v2/";
    public string AlbionDataProjectBaseUrlEast { get; set; } = "https://east.albion-online-data.com/api/v2/";
    public string AlbionOnlineApiBaseUrlWest { get; set; } = "https://gameinfo.albiononline.com";
    public string AlbionOnlineApiBaseUrlEast { get; set; } = "https://gameinfo-sgp.albiononline.com";
    public double MainWindowHeight { get; set; } = 100;
    public double MainWindowWidth { get; set; } = 100;
    public bool MainWindowMaximized { get; set; }
    public bool IsTrackingResetByMapChangeActive { get; set; }
    public bool IsMainTrackerFilterSilver { get; set; }
    public bool IsMainTrackerFilterFame { get; set; }
    public bool IsMainTrackerFilterFaction { get; set; }
    public bool IsMainTrackerFilterSeasonPoints { get; set; }
    public bool IsMainTrackerFilterEquipmentLoot { get; set; } = true;
    public bool IsMainTrackerFilterConsumableLoot { get; set; } = true;
    public bool IsMainTrackerFilterSimpleLoot { get; set; } = true;
    public bool IsMainTrackerFilterUnknownLoot { get; set; } = true;
    public bool IsMainTrackerFilterKill { get; set; }
    public bool IsDamageMeterTrackingActive { get; set; } = true;
    public bool IsTrackingPartyLootOnly { get; set; }
    public bool IsTrackingSilver { get; set; }
    public bool IsTrackingFame { get; set; }
    public bool IsTrackingMobLoot { get; set; }
    public bool IsLootLoggerSaveReminderActive { get; set; }
    public bool IsSuggestPreReleaseUpdatesActive { get; set; }
    public bool IsLootFromMobShown { get; set; }
    public double MailMonitoringGridSplitterPosition { get; set; } = 125;
    public double DungeonsGridSplitterPosition { get; set; } = 125;
    public double StorageHistoryGridSplitterPosition { get; set; } = 125;
    public double DamageMeterGridSplitterPosition { get; set; } = 125;
    public bool ShortDamageMeterToClipboard { get; set; }
    public bool IsTradeMonitoringActive { get; set; } = true;
    public bool IgnoreMailsWithZeroValues { get; set; }
    public int DeleteTradesOlderThanSpecifiedDays { get; set; }
    public bool IsSnapshotAfterMapChangeActive { get; set; }
    public bool IsDamageMeterResetByMapChangeActive { get; set; }
    public bool IsDamageMeterResetBeforeCombatActive { get; set; }
    public double TradeMonitoringMarketTaxRate { get; set; } = 4;
    public double TradeMonitoringMarketTaxSetupRate { get; set; } = 2.5;
    public bool IsDungeonClosedSoundActive { get; set; }
    public int ItemWindowMainTabQualitySelection { get; set; }
    public int ItemWindowHistoryTabQualitySelection { get; set; }
    public List<MainTabLocationFilterSettingsObject> ItemWindowMainTabLocationFilters { get; set; } = new();
    public double GatheringGridSplitterPosition { get; set; } = 125;
    public bool IsGatheringActive { get; set; }
    public bool IsDashboardNaviTabActive { get; set; } = true;
    public bool IsItemSearchNaviTabActive { get; set; } = true;
    public bool IsLoggingNaviTabActive { get; set; } = true;
    public bool IsDungeonsNaviTabActive { get; set; } = true;
    public bool IsDamageMeterNaviTabActive { get; set; } = true;
    public bool IsTradeMonitoringNaviTabActive { get; set; } = true;
    public bool IsGatheringNaviTabActive { get; set; } = true;
    public bool IsPartyPlannerNaviTabActive { get; set; } = true;
    public bool IsStorageHistoryNaviTabActive { get; set; } = true;
    public bool IsMapHistoryNaviTabActive { get; set; } = true;
    public bool IsPlayerInformationNaviTabActive { get; set; } = true;
    public bool IsNotificationFilterTradeActive { get; set; } = false;
    public bool IsNotificationTrackingStatusActive { get; set; } = false;
    public AutoDeleteGatheringStats AutoDeleteGatheringStats { get; set; } = AutoDeleteGatheringStats.NeverDelete;
    public short ExactMatchPlayerNamesLineNumber { get; set; } = 0;
    public DateTime TradeMonitoringDatePickerTradeFrom { get; set; } = new(2017, 1, 1);
    public DateTime TradeMonitoringDatePickerTradeTo { get; set; } = DateTime.UtcNow.AddDays(1);
}