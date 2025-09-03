using StatisticAnalysisTool.Extractor.Enums;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.PacketProviders;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common.UserSettings;

public class SettingsObject
{
    public string CurrentCultureIetfLanguageTag { get; set; }
    public int RefreshRate { get; set; } = 10000;
    public string PacketFilter { get; set; } = "(ip or ip6) and (udp and (port 5055 or port 5056 or port 5058))";
    public PacketProviderKind PacketProvider { get; set; } = PacketProviderKind.Npcap;
    public ServerLocation ServerLocation { get; set; } = 0; // 0: auto, 1: america, 2: asia, 3: europe
    public ServerType ServerType { get; set; } = ServerType.Live; // 0: Live, 1: Staging, 2: Playground
    public string MainTrackingCharacterName { get; set; }
    public int BackupIntervalByDays { get; set; } = 1;
    public int MaximumNumberOfBackups { get; set; } = 10;
    public string BackupStorageDirectoryPath { get; set; }
    public bool IsOpenItemWindowInNewWindowChecked { get; set; } = true;
    public bool IsInfoWindowShownOnStart { get; set; } = true;
    public string SelectedAlertSound { get; set; }
    public string SelectedDeathAlertSound { get; set; }
    public string AlbionDataProjectBaseUrlWest { get; set; } = "https://west.albion-online-data.com/api/v2/";
    public string AlbionDataProjectBaseUrlEast { get; set; } = "https://east.albion-online-data.com/api/v2/";
    public string AlbionDataProjectBaseUrlEurope { get; set; } = "https://europe.albion-online-data.com/api/v2/";
    public string AlbionOnlineApiBaseUrlWest { get; set; } = "https://gameinfo.albiononline.com";
    public string AlbionOnlineApiBaseUrlEast { get; set; } = "https://gameinfo-sgp.albiononline.com";
    public string AlbionOnlineApiBaseUrlEurope { get; set; } = "https://gameinfo-ams.albiononline.com";
    public double MainWindowHeight { get; set; } = 100;
    public double MainWindowWidth { get; set; } = 100;
    public double MainWindowLeftPosition { get; set; } = 0;
    public double MainWindowTopPosition { get; set; } = 0;
    public bool MainWindowMaximized { get; set; } = false;
    public bool IsTrackingResetByMapChangeActive { get; set; } = false;
    public bool IsMainTrackerFilterSilver { get; set; } = false;
    public bool IsMainTrackerFilterFame { get; set; } = false;
    public bool IsMainTrackerFilterFaction { get; set; } = false;
    public bool IsMainTrackerFilterSeasonPoints { get; set; } = false;
    public bool IsMainTrackerFilterEquipmentLoot { get; set; } = true;
    public bool IsMainTrackerFilterConsumableLoot { get; set; } = true;
    public bool IsMainTrackerFilterSimpleLoot { get; set; } = true;
    public bool IsMainTrackerFilterUnknownLoot { get; set; } = true;
    public bool IsMainTrackerFilterKill { get; set; } = false;
    public bool IsDamageMeterTrackingActive { get; set; } = true;
    public bool IsTrackingPartyLootOnly { get; set; } = false;
    public bool IsTrackingSilver { get; set; } = false;
    public bool IsTrackingFame { get; set; } = false;
    public bool IsTrackingMobLoot { get; set; } = false;
    public bool IsSuggestPreReleaseUpdatesActive { get; set; } = false;
    public bool IsLootFromMobShown { get; set; } = false;
    public double MailMonitoringGridSplitterPosition { get; set; } = 125;
    public double DungeonsGridSplitterPosition { get; set; } = 125;
    public double StorageHistoryGridSplitterPosition { get; set; } = 125;
    public double DamageMeterGridSplitterPosition { get; set; } = 125;
    public double PartyBuilderGridSplitterPosition { get; set; } = 125;
    public double GuildGridSplitterPosition { get; set; } = 125;
    public bool ShortDamageMeterToClipboard { get; set; } = false;
    public bool OnlyDamageToPlayersCounts { get; set; } = false;
    public bool IsTradeMonitoringActive { get; set; } = true;
    public bool IgnoreMailsWithZeroValues { get; set; } = false;
    public int DeleteTradesOlderThanSpecifiedDays { get; set; }
    public bool IsSnapshotAfterMapChangeActive { get; set; }
    public bool IsDamageMeterResetByMapChangeActive { get; set; }
    public bool IsDamageMeterResetBeforeCombatActive { get; set; }
    public double TradeMonitoringMarketTaxRate { get; set; } = 4;
    public double TradeMonitoringMarketTaxSetupRate { get; set; } = 2.5;
    public bool IsDungeonClosedSoundActive { get; set; } = false;
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
    public bool IsPartyNaviTabActive { get; set; } = true;
    public bool IsStorageHistoryNaviTabActive { get; set; } = true;
    public bool IsMapHistoryNaviTabActive { get; set; } = true;
    public bool IsPlayerInformationNaviTabActive { get; set; } = true;
    public bool IsGuildTabActive { get; set; } = true;
    public bool IsNotificationFilterTradeActive { get; set; } = false;
    public bool IsNotificationTrackingStatusActive { get; set; } = false;
    public AutoDeleteGatheringStats AutoDeleteGatheringStats { get; set; } = AutoDeleteGatheringStats.NeverDelete;
    public short ExactMatchPlayerNamesLineNumber { get; set; } = 0;
    public DateTime TradeMonitoringDatePickerTradeFrom { get; set; } = new(2017, 1, 1);
    public DateTime TradeMonitoringDatePickerTradeTo { get; set; } = DateTime.UtcNow.AddDays(1);
    public double PartyBuilderMinimalItemPower { get; set; } = 600;
    public double PartyBuilderMaximumItemPower { get; set; } = 900;
    public double PartyBuilderMinimalBasicItemPower { get; set; } = 600;
    public double PartyBuilderMaximumBasicItemPower { get; set; } = 900;
    public string AnotherAppToStartPath { get; set; }
    public string MainGameFolderPath { get; set; } = string.Empty;
    public bool IsKillDeathStatsVisible { get; set; } = true;
    public bool IsLootedChestsStatsVisible { get; set; } = true;
    public bool IsReSpecStatsVisible { get; set; } = true;
    public bool IsRepairCostsStatsVisible { get; set; } = true;
    public string ProxyUrlWithPort { get; set; }
    public string DebugConsoleFilter { get; set; }
    public bool IsOpenDebugConsoleWhenStartingTheToolChecked { get; set; } = false;
    public int NetworkDevice { get; set; } = -1;
}