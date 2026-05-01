using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class SettingsWindowViewModel : BaseViewModel
{
    private static ObservableCollection<FileInformation> _languages = [];
    private static FileInformation _languagesSelection;
    private static ObservableCollection<SettingDataInformation> _refreshRates = [];
    private static SettingDataInformation _refreshRatesSelection;

    public SettingsWindowViewModel()
    {
        InitializeSettings();
        Translation = new SettingsWindowTranslation();
    }

    private void InitializeSettings()
    {
        InitLanguageFiles();
        InitNaviTabVisibilities();
        InitNotificationAreas();
        InitRefreshRate();
        InitPacketProvider();
        InitNetworkDevices();
        InitServer();

        MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;

        // Debug console filter
        DebugConsoleFilter = SettingsController.CurrentSettings.DebugConsoleFilter;
        IsOpenDebugConsoleWhenStartingTheToolChecked = SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked;

        // Proxy url
        ProxyUrlWithPort = SettingsController.CurrentSettings.ProxyUrlWithPort;

        // Backup interval by days
        InitDropDownDownByDays(BackupIntervalByDays);
        BackupIntervalByDaysSelection = BackupIntervalByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.BackupIntervalByDays);

        // Maximum number of backups
        InitMaxAmountOfBackups(MaximumNumberOfBackups);
        MaximumNumberOfBackupsSelection = MaximumNumberOfBackups.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.MaximumNumberOfBackups);

        // Backup storage dir path
        BackupStorageDirectoryPath = AppDataPaths.BackupsDirectory;

        // Another app to start path
        AnotherAppToStartPath = SettingsController.CurrentSettings.AnotherAppToStartPath;

        // Alert sounds
        InitAlertSounds();

        // Api urls
        AlbionDataProjectBaseUrlWest = SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest;
        AlbionDataProjectBaseUrlEast = SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast;
        AlbionDataProjectBaseUrlEurope = SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEurope;

        // Auto update
        IsSuggestPreReleaseUpdatesActive = SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive;

        // Item window
        IsOpenItemWindowInNewWindowChecked = SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked;

        // Info window
        ShowInfoWindowOnStartChecked = SettingsController.CurrentSettings.IsInfoWindowShownOnStart;

        // Packet Filter
        PacketFilter = SettingsController.CurrentSettings.PacketFilter;

        // Player Selection with same name in db
        PlayerSelectionWithSameNameInDb = SettingsController.CurrentSettings.ExactMatchPlayerNamesLineNumber;

        // Another app to start
        SetIconSourceToAnotherAppToStart();
    }

    public async Task SaveSettingsAsync()
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        var oldPacketProvider = SettingsController.CurrentSettings.PacketProvider;
        var oldPacketFilter = SettingsController.CurrentSettings.PacketFilter;
        var oldNetworkDevices = GetNetworkDeviceSettingsSnapshot(SettingsController.CurrentSettings.NetworkDevices);

        SettingsController.CurrentSettings.RefreshRate = RefreshRatesSelection.Value;

        SettingsController.CurrentSettings.PacketProvider = (PacketProviderKind) PacketProviderSelection.Value;
        SettingsController.CurrentSettings.ServerLocation = (ServerLocation) ServerSelection.Value;
        SetPacketFilter();
        SetNetworkDevices();
        mainWindowViewModel.UpdateServerTypeLabel();

        SettingsController.CurrentSettings.AnotherAppToStartPath = AnotherAppToStartPath;

        SettingsController.CurrentSettings.DebugConsoleFilter = DebugConsoleFilter;
        SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked = IsOpenDebugConsoleWhenStartingTheToolChecked;
        SettingsController.CurrentSettings.ProxyUrlWithPort = ProxyUrlWithPort;
        SettingsController.CurrentSettings.MainTrackingCharacterName = MainTrackingCharacterName;
        SettingsController.CurrentSettings.BackupIntervalByDays = BackupIntervalByDaysSelection.Value;
        SettingsController.CurrentSettings.MaximumNumberOfBackups = MaximumNumberOfBackupsSelection.Value;
        SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;
        SettingsController.CurrentSettings.IsInfoWindowShownOnStart = ShowInfoWindowOnStartChecked;
        SettingsController.CurrentSettings.SelectedAlertSound = AlertSoundSelection?.FileName ?? string.Empty;
        SettingsController.CurrentSettings.SelectedDeathAlertSound = DeathAlertSoundSelection?.FileName ?? string.Empty;

        Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(LanguagesSelection.FileName));

        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest = AlbionDataProjectBaseUrlWest;
        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast = AlbionDataProjectBaseUrlEast;

        SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive = IsSuggestPreReleaseUpdatesActive;
        SettingsController.CurrentSettings.ExactMatchPlayerNamesLineNumber = PlayerSelectionWithSameNameInDb;

        SetBackupStorageDirPath();
        SetAppSettingsAndTranslations();
        SetNaviTabVisibilities(mainWindowViewModel);
        SetNotificationFilter();
        SetIconSourceToAnotherAppToStart();

        await SettingsController.SaveSettingsAsync();

        if (HaveNetworkTrackingSettingsChanged(oldPacketProvider, oldPacketFilter, oldNetworkDevices))
        {
            await RestartNetworkTrackingAsync();
        }
    }

    public void SaveSettings()
    {
        _ = SaveSettingsAsync();
    }

    public void ReloadSettings()
    {
        MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;
    }

    private void SetAppSettingsAndTranslations()
    {
        Translation = new SettingsWindowTranslation();
    }

    private void SetNaviTabVisibilities(MainWindowViewModel mainWindowViewModel)
    {
        SettingsController.CurrentSettings.IsDashboardNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Dashboard)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsItemSearchNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.ItemSearch)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsLoggingNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Logging)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsGuildTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Guild)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsDungeonsNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Dungeons)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsDamageMeterNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.DamageMeter)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.TradeMonitoring)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsGatheringNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Gathering)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsPartyNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Party)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.StorageHistory)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsMapHistoryNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.MapHistory)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.PlayerInformation)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsGameDataNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.GameData)?.IsSelected ?? true;

        mainWindowViewModel.DashboardTabVisibility = SettingsController.CurrentSettings.IsDashboardNaviTabActive.BoolToVisibility();
        mainWindowViewModel.ItemSearchTabVisibility = SettingsController.CurrentSettings.IsItemSearchNaviTabActive.BoolToVisibility();
        mainWindowViewModel.LoggingTabVisibility = SettingsController.CurrentSettings.IsLoggingNaviTabActive.BoolToVisibility();
        mainWindowViewModel.DungeonsTabVisibility = SettingsController.CurrentSettings.IsDungeonsNaviTabActive.BoolToVisibility();
        mainWindowViewModel.DamageMeterTabVisibility = SettingsController.CurrentSettings.IsDamageMeterNaviTabActive.BoolToVisibility();
        mainWindowViewModel.TradeMonitoringTabVisibility = SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GatheringTabVisibility = SettingsController.CurrentSettings.IsGatheringNaviTabActive.BoolToVisibility();
        mainWindowViewModel.PartyTabVisibility = SettingsController.CurrentSettings.IsPartyNaviTabActive.BoolToVisibility();
        mainWindowViewModel.StorageHistoryTabVisibility = SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive.BoolToVisibility();
        mainWindowViewModel.MapHistoryTabVisibility = SettingsController.CurrentSettings.IsMapHistoryNaviTabActive.BoolToVisibility();
        mainWindowViewModel.PlayerInformationTabVisibility = SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GameDataTabVisibility = SettingsController.CurrentSettings.IsGameDataNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GuildTabVisibility = SettingsController.CurrentSettings.IsGuildTabActive.BoolToVisibility();
    }

    private void SetPacketFilter()
    {
        if (SettingsController.CurrentSettings.PacketFilter == PacketFilter)
        {
            return;
        }

        SettingsController.CurrentSettings.PacketFilter = PacketFilter ?? string.Empty;
    }

    private void SetNetworkDevices()
    {
        if (NetworkDevices.Count == 0)
        {
            return;
        }

        SettingsController.CurrentSettings.NetworkDevices = NetworkDevices
            .Where(x => !string.IsNullOrWhiteSpace(x.Identifier))
            .Select(x => new NetworkDeviceSettingsObject
            {
                Identifier = x.Identifier,
                Name = x.Name,
                IsSelected = x.IsSelected == true
            })
            .ToList();
    }

    private static string GetNetworkDeviceSettingsSnapshot(IEnumerable<NetworkDeviceSettingsObject> networkDevices)
    {
        return string.Join("|", (networkDevices ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x?.Identifier))
            .OrderBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Identifier}:{x.IsSelected}"));
    }

    private bool HaveNetworkTrackingSettingsChanged(PacketProviderKind oldPacketProvider, string oldPacketFilter, string oldNetworkDevices)
    {
        var newNetworkDevices = GetNetworkDeviceSettingsSnapshot(SettingsController.CurrentSettings.NetworkDevices);

        return oldPacketProvider != SettingsController.CurrentSettings.PacketProvider
               || !string.Equals(oldPacketFilter, SettingsController.CurrentSettings.PacketFilter, StringComparison.Ordinal)
               || !string.Equals(oldNetworkDevices, newNetworkDevices, StringComparison.Ordinal);
    }

    public static async Task RestartNetworkTrackingAsync()
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();

        if (trackingController is null)
        {
            return;
        }

        await trackingController.RestartTrackingAsync();
    }

    public void ResetPacketFilter()
    {
        const string defaultFilter = "(ip or ip6) and (udp and (port 5055 or port 5056 or port 5058))";

        if (PacketFilter == defaultFilter)
        {
            return;
        }

        PacketFilter = defaultFilter;
    }

    private void SetNotificationFilter()
    {
        SettingsController.CurrentSettings.IsNotificationFilterTradeActive = NotificationFilters?.FirstOrDefault(x => x?.NotificationFilterType == NotificationFilterType.Trade)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsNotificationTrackingStatusActive = NotificationFilters?.FirstOrDefault(x => x?.NotificationFilterType == NotificationFilterType.TrackingStatus)?.IsSelected ?? true;
    }

    public void ResetPlayerSelectionWithSameNameInDb()
    {
        const short defaultValue = 0;

        if (PlayerSelectionWithSameNameInDb == defaultValue)
        {
            return;
        }

        PlayerSelectionWithSameNameInDb = defaultValue;
    }

    private void SetIconSourceToAnotherAppToStart()
    {
        AnotherAppToStartExeIcon = GetExeIcon(SettingsController.CurrentSettings.AnotherAppToStartPath);
    }

    private static BitmapImage GetExeIcon(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            Icon appIcon = Icon.ExtractAssociatedIcon(path);
            Icon highDpiIcon = appIcon;
            BitmapImage imageResult;

            if (appIcon != null && appIcon.Handle != IntPtr.Zero)
            {
                highDpiIcon = Icon.FromHandle(new Icon(appIcon, new System.Drawing.Size(64, 64)).Handle);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                highDpiIcon?.Save(stream);

                stream.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                imageResult = bitmapImage;
            }

            highDpiIcon?.Dispose();
            appIcon?.Dispose();

            return imageResult;
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public struct SettingDataInformation
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public struct SettingDataStringInformation
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public static void OpenEventValidationWindow()
    {
        try
        {
            if (Utilities.IsWindowOpen<EventValidationWindow>())
            {
                var existWindow = Application.Current.Windows.OfType<EventValidationWindow>().FirstOrDefault();
                existWindow?.Activate();
            }
            else
            {
                var window = new EventValidationWindow();
                window.Show();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private void SetBackupStorageDirPath()
    {
        BackupStorageDirectoryPath = AppDataPaths.BackupsDirectory;
        SettingsController.CurrentSettings.BackupStorageDirectoryPath = AppDataPaths.BackupsDirectory;
    }

    public void ResetBackupStorageDirPath()
    {
        if (BackupStorageDirectoryPath == AppDataPaths.BackupsDirectory)
        {
            return;
        }

        BackupStorageDirectoryPath = AppDataPaths.BackupsDirectory;
    }

    #region Inits

    private void InitLanguageFiles()
    {
        Languages = new ObservableCollection<FileInformation>(LocalizationController.GetLanguageInformation());
        LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == CultureInfo.DefaultThreadCurrentCulture?.TextInfo.CultureName);
    }

    private void InitNaviTabVisibilities()
    {
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Dashboard)
        {
            IsSelected = SettingsController.CurrentSettings.IsDashboardNaviTabActive,
            Name = MainWindowTranslation.Dashboard
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.ItemSearch)
        {
            IsSelected = SettingsController.CurrentSettings.IsItemSearchNaviTabActive,
            Name = MainWindowTranslation.ItemSearch
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Logging)
        {
            IsSelected = SettingsController.CurrentSettings.IsLoggingNaviTabActive,
            Name = MainWindowTranslation.Logging
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Guild)
        {
            IsSelected = SettingsController.CurrentSettings.IsGuildTabActive,
            Name = MainWindowTranslation.Guild
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Dungeons)
        {
            IsSelected = SettingsController.CurrentSettings.IsDungeonsNaviTabActive,
            Name = MainWindowTranslation.Dungeons
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.DamageMeter)
        {
            IsSelected = SettingsController.CurrentSettings.IsDamageMeterNaviTabActive,
            Name = MainWindowTranslation.DamageMeter
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.TradeMonitoring)
        {
            IsSelected = SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive,
            Name = MainWindowTranslation.TradeMonitoring
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Gathering)
        {
            IsSelected = SettingsController.CurrentSettings.IsGatheringNaviTabActive,
            Name = MainWindowTranslation.Gathering
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.Party)
        {
            IsSelected = SettingsController.CurrentSettings.IsPartyNaviTabActive,
            Name = MainWindowTranslation.Party
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.StorageHistory)
        {
            IsSelected = SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive,
            Name = MainWindowTranslation.StorageHistory
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.MapHistory)
        {
            IsSelected = SettingsController.CurrentSettings.IsMapHistoryNaviTabActive,
            Name = MainWindowTranslation.MapHistory
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.PlayerInformation)
        {
            IsSelected = SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive,
            Name = MainWindowTranslation.PlayerInformation
        });
        TabVisibilities.Add(new TabVisibilityFilter(NavigationTabFilterType.GameData)
        {
            IsSelected = SettingsController.CurrentSettings.IsGameDataNaviTabActive,
            Name = MainWindowTranslation.GameData
        });

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.DashboardTabVisibility = SettingsController.CurrentSettings.IsDashboardNaviTabActive.BoolToVisibility();
        mainWindowViewModel.ItemSearchTabVisibility = SettingsController.CurrentSettings.IsItemSearchNaviTabActive.BoolToVisibility();
        mainWindowViewModel.LoggingTabVisibility = SettingsController.CurrentSettings.IsLoggingNaviTabActive.BoolToVisibility();
        mainWindowViewModel.DungeonsTabVisibility = SettingsController.CurrentSettings.IsDungeonsNaviTabActive.BoolToVisibility();
        mainWindowViewModel.DamageMeterTabVisibility = SettingsController.CurrentSettings.IsDamageMeterNaviTabActive.BoolToVisibility();
        mainWindowViewModel.TradeMonitoringTabVisibility = SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GatheringTabVisibility = SettingsController.CurrentSettings.IsGatheringNaviTabActive.BoolToVisibility();
        mainWindowViewModel.PartyTabVisibility = SettingsController.CurrentSettings.IsPartyNaviTabActive.BoolToVisibility();
        mainWindowViewModel.StorageHistoryTabVisibility = SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive.BoolToVisibility();
        mainWindowViewModel.MapHistoryTabVisibility = SettingsController.CurrentSettings.IsMapHistoryNaviTabActive.BoolToVisibility();
        mainWindowViewModel.PlayerInformationTabVisibility = SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GameDataTabVisibility = SettingsController.CurrentSettings.IsGameDataNaviTabActive.BoolToVisibility();
        mainWindowViewModel.GuildTabVisibility = SettingsController.CurrentSettings.IsGuildTabActive.BoolToVisibility();
    }

    private void InitNotificationAreas()
    {
        NotificationFilters.Add(new NotificationFilter(NotificationFilterType.Trade)
        {
            IsSelected = SettingsController.CurrentSettings.IsNotificationFilterTradeActive,
            Name = LocalizationController.Translation("ADDED_TRADES")
        });

        NotificationFilters.Add(new NotificationFilter(NotificationFilterType.TrackingStatus)
        {
            IsSelected = SettingsController.CurrentSettings.IsNotificationTrackingStatusActive,
            Name = LocalizationController.Translation("TRACKING_STATUS")
        });
    }

    private void InitRefreshRate()
    {
        RefreshRates.Clear();
        RefreshRates.Add(new SettingDataInformation { Name = SettingsWindowTranslation.FiveSeconds, Value = 5000 });
        RefreshRates.Add(new SettingDataInformation { Name = SettingsWindowTranslation.TenSeconds, Value = 10000 });
        RefreshRates.Add(new SettingDataInformation { Name = SettingsWindowTranslation.ThirtySeconds, Value = 30000 });
        RefreshRates.Add(new SettingDataInformation { Name = SettingsWindowTranslation.SixtySeconds, Value = 60000 });
        RefreshRates.Add(new SettingDataInformation { Name = SettingsWindowTranslation.FiveMinutes, Value = 300000 });
        RefreshRatesSelection = RefreshRates.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.RefreshRate);
    }

    private void InitPacketProvider()
    {
        PacketProvider.Clear();
        PacketProvider.Add(new SettingDataInformation { Name = $"Sockets ({LocalizationController.Translation("TOOL_MUST_BE_RUN_AS_ADMIN")})", Value = (int) PacketProviderKind.Sockets });
        PacketProvider.Add(new SettingDataInformation { Name = "Npcap", Value = (int) PacketProviderKind.Npcap });
        PacketProviderSelection = PacketProvider.FirstOrDefault(x => x.Value == (int) SettingsController.CurrentSettings.PacketProvider);
    }

    private void InitNetworkDevices()
    {
        NetworkDevices.Clear();

        try
        {
            var availableDevices = LibpcapPacketProvider.GetAvailableNetworkDevices();
            var configuredDevices = SettingsController.CurrentSettings.NetworkDevices ?? new List<NetworkDeviceSettingsObject>();
            var configuredByIdentifier = configuredDevices
                .Where(x => !string.IsNullOrWhiteSpace(x?.Identifier))
                .GroupBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            bool hasConfiguredDevices = configuredByIdentifier.Count > 0;
            int legacyNetworkDeviceIndex = SettingsController.CurrentSettings.NetworkDevice;

            foreach (var availableDevice in availableDevices)
            {
                bool isSelected;

                if (hasConfiguredDevices && configuredByIdentifier.TryGetValue(availableDevice.Identifier, out var configuredDevice))
                {
                    isSelected = configuredDevice.IsSelected;
                }
                else if (!hasConfiguredDevices && legacyNetworkDeviceIndex >= 0)
                {
                    isSelected = availableDevice.Index == legacyNetworkDeviceIndex;
                }
                else
                {
                    isSelected = true;
                }

                NetworkDevices.Add(new NetworkDeviceFilter
                {
                    Identifier = availableDevice.Identifier,
                    Index = availableDevice.Index,
                    IsSelected = isSelected,
                    Name = availableDevice.Name
                });
            }

            SetNetworkDevices();
        }
        catch (Exception e)
        {
            Log.Warning(e, "Network devices could not be loaded from Npcap");
        }
    }

    private void InitServer()
    {
        Server.Clear();
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.WestServer, Value = 1 });
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.EastServer, Value = 2 });
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.EuropeServer, Value = 3 });
        ServerSelection = Server.FirstOrDefault(x => x.Value == (int) SettingsController.CurrentSettings.ServerLocation);
    }

    private void InitMaxAmountOfBackups(ICollection<SettingDataInformation> amountOfBackups)
    {
        amountOfBackups.Clear();
        amountOfBackups.Add(new SettingDataInformation { Name = "5", Value = 5 });
        amountOfBackups.Add(new SettingDataInformation { Name = "10", Value = 10 });
        amountOfBackups.Add(new SettingDataInformation { Name = "20", Value = 20 });
        amountOfBackups.Add(new SettingDataInformation { Name = "50", Value = 50 });
        amountOfBackups.Add(new SettingDataInformation { Name = "100", Value = 100 });
        amountOfBackups.Add(new SettingDataInformation { Name = "250", Value = 250 });
    }

    private static void InitDropDownDownByDays(ICollection<SettingDataInformation> updateJsonByDays)
    {
        updateJsonByDays.Clear();
        updateJsonByDays.Add(new SettingDataInformation { Name = LocalizationController.Translation("EVERY_DAY"), Value = 1 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LocalizationController.Translation("EVERY_3_DAYS"), Value = 3 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LocalizationController.Translation("EVERY_7_DAYS"), Value = 7 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LocalizationController.Translation("EVERY_14_DAYS"), Value = 14 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LocalizationController.Translation("EVERY_28_DAYS"), Value = 28 });
    }

    private void InitAlertSounds()
    {
        SoundController.InitializeSoundFilesFromDirectory();

        // Item alert sounds
        AlertSounds.Clear();
        foreach (var sound in SoundController.Sounds.Where(x => x.FileName.Contains("alert") && !x.FileName.Contains("deathalert")))
        {
            AlertSounds.Add(new FileInformation(sound.FileName, sound.FilePath));
        }

        AlertSoundSelection = AlertSounds.FirstOrDefault(x => x.FileName == SettingsController.CurrentSettings.SelectedAlertSound);

        // Death alert sounds
        DeathAlertSounds.Clear();
        foreach (var sound in SoundController.Sounds.Where(x => x.FileName.Contains("deathalert")))
        {
            DeathAlertSounds.Add(new FileInformation(sound.FileName, sound.FilePath));
        }

        DeathAlertSoundSelection = DeathAlertSounds.FirstOrDefault(x => x.FileName == SettingsController.CurrentSettings.SelectedDeathAlertSound);
    }

    #endregion

    #region Bindings

    public ObservableCollection<NotificationFilter> NotificationFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<TabVisibilityFilter> TabVisibilities
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<FileInformation> AlertSounds
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<FileInformation> DeathAlertSounds
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public FileInformation AlertSoundSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public FileInformation DeathAlertSoundSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation BackupIntervalByDaysSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation MaximumNumberOfBackupsSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> BackupIntervalByDays
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<SettingDataInformation> MaximumNumberOfBackups
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public string BackupStorageDirectoryPath
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }


    public SettingDataInformation RefreshRatesSelection
    {
        get => _refreshRatesSelection;
        set
        {
            _refreshRatesSelection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> RefreshRates
    {
        get => _refreshRates;
        set
        {
            _refreshRates = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation PacketProviderSelection
    {
        get;
        set
        {
            field = value;
            PacketFilterVisibility = field.Value == 2 ? Visibility.Visible : Visibility.Collapsed;
            NetworkDevicesVisibility = field.Value == 2 ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> PacketProvider
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public Visibility PacketFilterVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility NetworkDevicesVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public ObservableCollection<NetworkDeviceFilter> NetworkDevices
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public SettingDataInformation ServerSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> Server
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public string PacketFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public short PlayerSelectionWithSameNameInDb
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string MainTrackingCharacterName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ProxyUrlWithPort
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string DebugConsoleFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public FileInformation LanguagesSelection
    {
        get => _languagesSelection;
        set
        {
            _languagesSelection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FileInformation> Languages
    {
        get => _languages;
        set
        {
            _languages = value;
            OnPropertyChanged();
        }
    }

    public string AnotherAppToStartPath
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SettingsWindowTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsOpenItemWindowInNewWindowChecked
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsOpenDebugConsoleWhenStartingTheToolChecked
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowInfoWindowOnStartChecked
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlWest
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlEast
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlEurope
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsSuggestPreReleaseUpdatesActive
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsBackupNowButtonEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public BitmapImage AnotherAppToStartExeIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ToolDirectory => AppDataPaths.InstallationDirectory;

    #endregion Bindings
}
