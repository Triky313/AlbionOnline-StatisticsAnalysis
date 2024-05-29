using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class SettingsWindowViewModel : BaseViewModel
{
    private static ObservableCollection<FileInformation> _languages = new();
    private static FileInformation _languagesSelection;
    private static ObservableCollection<SettingDataInformation> _refreshRates = new();
    private static SettingDataInformation _refreshRatesSelection;

    private ObservableCollection<FileInformation> _alertSounds = new();
    private ObservableCollection<FileInformation> _deathAlertSounds = new();
    private FileInformation _alertSoundSelection;
    private FileInformation _deathAlertSoundSelection;
    private bool _isOpenItemWindowInNewWindowChecked;
    private bool _showInfoWindowOnStartChecked;
    private SettingsWindowTranslation _translation;
    private string _albionDataProjectBaseUrlWest;
    private string _albionDataProjectBaseUrlEast;
    private string _albionDataProjectBaseUrlEurope;
    private ObservableCollection<SettingDataInformation> _backupIntervalByDays = new();
    private ObservableCollection<SettingDataInformation> _maximumNumberOfBackups = new();
    private bool _isSuggestPreReleaseUpdatesActive;
    private string _mainTrackingCharacterName;
    private ObservableCollection<TabVisibilityFilter> _tabVisibilities = new();
    private SettingDataInformation _packetProviderSelection;
    private ObservableCollection<SettingDataInformation> _packetProvider = new();
    private SettingDataInformation _serverSelection;
    private ObservableCollection<SettingDataInformation> _server = new();
    private ObservableCollection<NotificationFilter> _notificationFilters = new();
    private short _playerSelectionWithSameNameInDb;
    private bool _isBackupNowButtonEnabled = true;
    private SettingDataInformation _backupIntervalByDaysSelection;
    private SettingDataInformation _maximumNumberOfBackupsSelection;
    private string _anotherAppToStartPath;
    private BitmapImage _anotherAppToStartExeIcon;
    private string _packetFilter;
    private Visibility _packetFilterVisibility = Visibility.Collapsed;

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
        InitServer();

        MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;

        // Backup interval by days
        InitDropDownDownByDays(BackupIntervalByDays);
        BackupIntervalByDaysSelection = BackupIntervalByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.BackupIntervalByDays);

        // Maximum number of backups
        InitMaxAmountOfBackups(MaximumNumberOfBackups);
        MaximumNumberOfBackupsSelection = MaximumNumberOfBackups.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.MaximumNumberOfBackups);

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

    public void SaveSettings()
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

        SettingsController.CurrentSettings.RefreshRate = RefreshRatesSelection.Value;

        SettingsController.CurrentSettings.PacketProvider = (PacketProviderKind) PacketProviderSelection.Value;
        SettingsController.CurrentSettings.ServerLocation = (ServerLocation) ServerSelection.Value;
        SetPacketFilter();
        mainWindowViewModel.UpdateServerTypeLabel();

        SettingsController.CurrentSettings.AnotherAppToStartPath = AnotherAppToStartPath;

        SettingsController.CurrentSettings.MainTrackingCharacterName = MainTrackingCharacterName;
        SettingsController.CurrentSettings.BackupIntervalByDays = BackupIntervalByDaysSelection.Value;
        SettingsController.CurrentSettings.MaximumNumberOfBackups = MaximumNumberOfBackupsSelection.Value;
        SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;
        SettingsController.CurrentSettings.IsInfoWindowShownOnStart = ShowInfoWindowOnStartChecked;
        SettingsController.CurrentSettings.SelectedAlertSound = AlertSoundSelection?.FileName ?? string.Empty;
        SettingsController.CurrentSettings.SelectedDeathAlertSound = DeathAlertSoundSelection?.FileName ?? string.Empty;

        LanguageController.SetLanguage(Culture.GetCulture(LanguagesSelection.FileName));

        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest = AlbionDataProjectBaseUrlWest;
        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast = AlbionDataProjectBaseUrlEast;

        SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive = IsSuggestPreReleaseUpdatesActive;
        SettingsController.CurrentSettings.ExactMatchPlayerNamesLineNumber = PlayerSelectionWithSameNameInDb;

        SetAppSettingsAndTranslations();
        SetNaviTabVisibilities(mainWindowViewModel);
        SetNotificationFilter();
        SetIconSourceToAnotherAppToStart();
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

    public void ResetPacketFilter()
    {
        const string defaultFilter = "(host 5.45.187 or host 5.188.125 or 193.169.238) and udp port 5056";

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

    public static void OpenConsoleWindow()
    {
        try
        {
            if (Utilities.IsWindowOpen<ConsoleWindow>())
            {
                var existWindow = Application.Current.Windows.OfType<ConsoleWindow>().FirstOrDefault();
                existWindow?.Activate();
            }
            else
            {
                var window = new ConsoleWindow();
                window.Show();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    #region Inits

    private void InitLanguageFiles()
    {
        Languages = new ObservableCollection<FileInformation>(LanguageController.InitLanguageFiles());
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
        mainWindowViewModel.GuildTabVisibility = SettingsController.CurrentSettings.IsGuildTabActive.BoolToVisibility();
    }

    private void InitNotificationAreas()
    {
        NotificationFilters.Add(new NotificationFilter(NotificationFilterType.Trade)
        {
            IsSelected = SettingsController.CurrentSettings.IsNotificationFilterTradeActive,
            Name = LanguageController.Translation("ADDED_TRADES")
        });

        NotificationFilters.Add(new NotificationFilter(NotificationFilterType.TrackingStatus)
        {
            IsSelected = SettingsController.CurrentSettings.IsNotificationTrackingStatusActive,
            Name = LanguageController.Translation("TRACKING_STATUS")
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
        PacketProvider.Add(new SettingDataInformation { Name = $"Sockets ({LanguageController.Translation("TOOL_MUST_BE_RUN_AS_ADMIN")})", Value = (int) PacketProviderKind.Sockets });
        PacketProvider.Add(new SettingDataInformation { Name = $"Npcap ({LanguageController.Translation("EXPERIMENTAL")})", Value = (int) PacketProviderKind.Npcap });
        PacketProviderSelection = PacketProvider.FirstOrDefault(x => x.Value == (int) SettingsController.CurrentSettings.PacketProvider);
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
        updateJsonByDays.Add(new SettingDataInformation { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
        updateJsonByDays.Add(new SettingDataInformation { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
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
        get => _notificationFilters;
        set
        {
            _notificationFilters = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TabVisibilityFilter> TabVisibilities
    {
        get => _tabVisibilities;
        set
        {
            _tabVisibilities = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FileInformation> AlertSounds
    {
        get => _alertSounds;
        set
        {
            _alertSounds = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FileInformation> DeathAlertSounds
    {
        get => _deathAlertSounds;
        set
        {
            _deathAlertSounds = value;
            OnPropertyChanged();
        }
    }

    public FileInformation AlertSoundSelection
    {
        get => _alertSoundSelection;
        set
        {
            _alertSoundSelection = value;
            OnPropertyChanged();
        }
    }

    public FileInformation DeathAlertSoundSelection
    {
        get => _deathAlertSoundSelection;
        set
        {
            _deathAlertSoundSelection = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation BackupIntervalByDaysSelection
    {
        get => _backupIntervalByDaysSelection;
        set
        {
            _backupIntervalByDaysSelection = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation MaximumNumberOfBackupsSelection
    {
        get => _maximumNumberOfBackupsSelection;
        set
        {
            _maximumNumberOfBackupsSelection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> BackupIntervalByDays
    {
        get => _backupIntervalByDays;
        set
        {
            _backupIntervalByDays = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> MaximumNumberOfBackups
    {
        get => _maximumNumberOfBackups;
        set
        {
            _maximumNumberOfBackups = value;
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
        get => _packetProviderSelection;
        set
        {
            _packetProviderSelection = value;
            PacketFilterVisibility = _packetProviderSelection.Value == 2 ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> PacketProvider
    {
        get => _packetProvider;
        set
        {
            _packetProvider = value;
            OnPropertyChanged();
        }
    }

    public Visibility PacketFilterVisibility
    {
        get => _packetFilterVisibility;
        set
        {
            _packetFilterVisibility = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation ServerSelection
    {
        get => _serverSelection;
        set
        {
            _serverSelection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> Server
    {
        get => _server;
        set
        {
            _server = value;
            OnPropertyChanged();
        }
    }

    public string PacketFilter
    {
        get => _packetFilter;
        set
        {
            _packetFilter = value;
            OnPropertyChanged();
        }
    }

    public short PlayerSelectionWithSameNameInDb
    {
        get => _playerSelectionWithSameNameInDb;
        set
        {
            _playerSelectionWithSameNameInDb = value;
            OnPropertyChanged();
        }
    }

    public string MainTrackingCharacterName
    {
        get => _mainTrackingCharacterName;
        set
        {
            _mainTrackingCharacterName = value;
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
        get => _anotherAppToStartPath;
        set
        {
            _anotherAppToStartPath = value;
            OnPropertyChanged();
        }
    }

    public SettingsWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    public bool IsOpenItemWindowInNewWindowChecked
    {
        get => _isOpenItemWindowInNewWindowChecked;
        set
        {
            _isOpenItemWindowInNewWindowChecked = value;
            OnPropertyChanged();
        }
    }

    public bool ShowInfoWindowOnStartChecked
    {
        get => _showInfoWindowOnStartChecked;
        set
        {
            _showInfoWindowOnStartChecked = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlWest
    {
        get => _albionDataProjectBaseUrlWest;
        set
        {
            _albionDataProjectBaseUrlWest = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlEast
    {
        get => _albionDataProjectBaseUrlEast;
        set
        {
            _albionDataProjectBaseUrlEast = value;
            OnPropertyChanged();
        }
    }

    public string AlbionDataProjectBaseUrlEurope
    {
        get => _albionDataProjectBaseUrlEurope;
        set
        {
            _albionDataProjectBaseUrlEurope = value;
            OnPropertyChanged();
        }
    }

    public bool IsSuggestPreReleaseUpdatesActive
    {
        get => _isSuggestPreReleaseUpdatesActive;
        set
        {
            _isSuggestPreReleaseUpdatesActive = value;
            OnPropertyChanged();
        }
    }

    public bool IsBackupNowButtonEnabled
    {
        get => _isBackupNowButtonEnabled;
        set
        {
            _isBackupNowButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage AnotherAppToStartExeIcon
    {
        get => _anotherAppToStartExeIcon;
        set
        {
            _anotherAppToStartExeIcon = value;
            OnPropertyChanged();
        }
    }

    public string ToolDirectory => AppDomain.CurrentDomain.BaseDirectory;

    #endregion Bindings
}