using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Network;

namespace StatisticsAnalysisTool.ViewModels;

public class SettingsWindowViewModel : INotifyPropertyChanged
{
    private static string _itemListSourceUrl;
    private static string _mobsJsonSourceUrl;
    private static ObservableCollection<FileInformation> _languages = new();
    private static FileInformation _languagesSelection;
    private static ObservableCollection<SettingDataInformation> _refreshRates = new();
    private static SettingDataInformation _refreshRatesSelection;
    private static ObservableCollection<SettingDataInformation> _updateItemListByDays = new();
    private static SettingDataInformation _updateItemListByDaysSelection;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private ObservableCollection<FileInformation> _alertSounds = new();
    private FileInformation _alertSoundSelection;
    private bool _isOpenItemWindowInNewWindowChecked;
    private bool _showInfoWindowOnStartChecked;
    private SettingsWindowTranslation _translation;
    private string _albionDataProjectBaseUrlWest;
    private string _albionDataProjectBaseUrlEast;
    private string _goldStatsApiUrl;
    private bool _isLootLoggerSaveReminderActive;
    private string _itemsJsonSourceUrl;
    private ObservableCollection<SettingDataInformation> _updateItemsJsonByDays = new();
    private ObservableCollection<SettingDataInformation> _updateMobsJsonByDays = new();
    private SettingDataInformation _updateItemsJsonByDaysSelection;
    private SettingDataInformation _updateMobsJsonByDaysSelection;
    private bool _isSuggestPreReleaseUpdatesActive;
    private string _mainTrackingCharacterName;
    private bool _shortDamageMeterToClipboard;
    private ObservableCollection<TabVisibilityFilter> _tabVisibilities = new();
    private SettingDataInformation _serverSelection;
    private ObservableCollection<SettingDataInformation> _server = new();

    public SettingsWindowViewModel()
    {
        InitializeSettings();
        Translation = new SettingsWindowTranslation();
    }

    private void InitializeSettings()
    {
        InitLanguageFiles();
        InitNaviTabVisibilities();
        InitRefreshRate();
        InitServer();

        MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;

        // Update item list by days
        InitDropDownDownByDays(UpdateItemListByDays);
        UpdateItemListByDaysSelection = UpdateItemListByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateItemListByDays);
        ItemListSourceUrl = SettingsController.CurrentSettings.ItemListSourceUrl;

        // Update items.json by days
        InitDropDownDownByDays(UpdateItemsJsonByDays);
        UpdateItemsJsonByDaysSelection = UpdateItemsJsonByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateItemsJsonByDays);
        ItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemsJsonSourceUrl;

        // Update mobs.json by days
        InitDropDownDownByDays(UpdateMobsJsonByDays);
        UpdateMobsJsonByDaysSelection = UpdateMobsJsonByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateMobsJsonByDays);
        MobsJsonSourceUrl = SettingsController.CurrentSettings.MobsJsonSourceUrl;

        // Alert sounds
        InitAlertSounds();

        // Api urls
        AlbionDataProjectBaseUrlWest = SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest;
        AlbionDataProjectBaseUrlEast = SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast;

        // Loot logger
        IsLootLoggerSaveReminderActive = SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive;

        // Auto update
        IsSuggestPreReleaseUpdatesActive = SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive;

        // Damage Meter
        ShortDamageMeterToClipboard = SettingsController.CurrentSettings.ShortDamageMeterToClipboard;

        // Item window
        IsOpenItemWindowInNewWindowChecked = SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked;

        // Info window
        ShowInfoWindowOnStartChecked = SettingsController.CurrentSettings.IsInfoWindowShownOnStart;
    }

    public void SaveSettings()
    {
        SettingsController.CurrentSettings.ItemListSourceUrl = ItemListSourceUrl;
        SettingsController.CurrentSettings.ItemsJsonSourceUrl = ItemsJsonSourceUrl;
        SettingsController.CurrentSettings.MobsJsonSourceUrl = MobsJsonSourceUrl;
        SettingsController.CurrentSettings.RefreshRate = RefreshRatesSelection.Value;
        SettingsController.CurrentSettings.Server = ServerSelection.Value;
        NetworkManager.SetCurrentServer(ServerSelection.Value >= 2 ? AlbionServer.East : AlbionServer.West, true);
        SettingsController.CurrentSettings.MainTrackingCharacterName = MainTrackingCharacterName;
        SettingsController.CurrentSettings.UpdateItemListByDays = UpdateItemListByDaysSelection.Value;
        SettingsController.CurrentSettings.UpdateItemsJsonByDays = UpdateItemsJsonByDaysSelection.Value;
        SettingsController.CurrentSettings.UpdateMobsJsonByDays = UpdateMobsJsonByDaysSelection.Value;
        SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;
        SettingsController.CurrentSettings.IsInfoWindowShownOnStart = ShowInfoWindowOnStartChecked;
        SettingsController.CurrentSettings.SelectedAlertSound = AlertSoundSelection?.FileName ?? string.Empty;

        LanguageController.CurrentCultureInfo = new CultureInfo(LanguagesSelection.FileName);
        LanguageController.SetLanguage();

        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlWest = AlbionDataProjectBaseUrlWest;
        SettingsController.CurrentSettings.AlbionDataProjectBaseUrlEast = AlbionDataProjectBaseUrlEast;

        SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive = IsLootLoggerSaveReminderActive;
        SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive = IsSuggestPreReleaseUpdatesActive;
        SettingsController.CurrentSettings.ShortDamageMeterToClipboard = ShortDamageMeterToClipboard;

        SetAppSettingsAndTranslations();
        SetNaviTabVisibilities();
    }

    public void ReloadSettings()
    {
        MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;
    }

    private void SetAppSettingsAndTranslations()
    {
        Translation = new SettingsWindowTranslation();
    }

    private void SetNaviTabVisibilities()
    {
        SettingsController.CurrentSettings.IsDashboardNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Dashboard)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsItemSearchNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.ItemSearch)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsLoggingNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Logging)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsDungeonsNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Dungeons)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsDamageMeterNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.DamageMeter)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsTradeMonitoringNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.TradeMonitoring)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsGatheringNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.Gathering)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsStorageHistoryNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.StorageHistory)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsMapHistoryNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.MapHistory)?.IsSelected ?? true;
        SettingsController.CurrentSettings.IsPlayerInformationNaviTabActive = TabVisibilities?.FirstOrDefault(x => x?.NavigationTabFilterType == NavigationTabFilterType.PlayerInformation)?.IsSelected ?? true;
    }

    public struct SettingDataInformation
    {
        public string Name { get; set; }
        public int Value { get; set; }
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
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #region Inits

    private void InitLanguageFiles()
    {
        Languages.Clear();

        LanguageController.GetPercentageTranslationValues();

        foreach (var langInfo in LanguageController.LanguageFiles)
        {
            try
            {
                var cultureInfo = CultureInfo.CreateSpecificCulture(langInfo.FileName);
                Languages.Add(new FileInformation(langInfo.FileName, string.Empty)
                {
                    EnglishName = cultureInfo.EnglishName,
                    NativeName = cultureInfo.NativeName,
                    PercentageTranslations = langInfo.PercentageTranslations
                });
            }
            catch (CultureNotFoundException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentCultureInfo.TextInfo.CultureName);
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

    private void InitServer()
    {
        Server.Clear();
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.Automatically, Value = 0 });
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.WestServer, Value = 1 });
        Server.Add(new SettingDataInformation { Name = SettingsWindowTranslation.EastServer, Value = 2 });
        ServerSelection = Server.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.Server);
    }

    private void InitDropDownDownByDays(ICollection<SettingDataInformation> updateJsonByDays)
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
        AlertSounds.Clear();
        SoundController.InitializeSoundFilesFromDirectory();
        foreach (var sound in SoundController.AlertSounds)
        {
            AlertSounds.Add(new FileInformation(sound.FileName, sound.FilePath));
        }

        AlertSoundSelection = AlertSounds.FirstOrDefault(x => x.FileName == SettingsController.CurrentSettings.SelectedAlertSound);
    }

    #endregion

    #region Bindings

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

    public FileInformation AlertSoundSelection
    {
        get => _alertSoundSelection;
        set
        {
            _alertSoundSelection = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation UpdateItemListByDaysSelection
    {
        get => _updateItemListByDaysSelection;
        set
        {
            _updateItemListByDaysSelection = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation UpdateItemsJsonByDaysSelection
    {
        get => _updateItemsJsonByDaysSelection;
        set
        {
            _updateItemsJsonByDaysSelection = value;
            OnPropertyChanged();
        }
    }

    public SettingDataInformation UpdateMobsJsonByDaysSelection
    {
        get => _updateMobsJsonByDaysSelection;
        set
        {
            _updateMobsJsonByDaysSelection = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> UpdateItemListByDays
    {
        get => _updateItemListByDays;
        set
        {
            _updateItemListByDays = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> UpdateItemsJsonByDays
    {
        get => _updateItemsJsonByDays;
        set
        {
            _updateItemsJsonByDays = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SettingDataInformation> UpdateMobsJsonByDays
    {
        get => _updateMobsJsonByDays;
        set
        {
            _updateMobsJsonByDays = value;
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

    public SettingDataInformation ServerSelection
    {
        get => _serverSelection;
        set
        {
            _serverSelection = value;
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

    public ObservableCollection<SettingDataInformation> Server
    {
        get => _server;
        set
        {
            _server = value;
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

    public string ItemListSourceUrl
    {
        get => _itemListSourceUrl;
        set
        {
            _itemListSourceUrl = value;
            OnPropertyChanged();
        }
    }

    public string ItemsJsonSourceUrl
    {
        get => _itemsJsonSourceUrl;
        set
        {
            _itemsJsonSourceUrl = value;
            OnPropertyChanged();
        }
    }

    public string MobsJsonSourceUrl
    {
        get => _mobsJsonSourceUrl;
        set
        {
            _mobsJsonSourceUrl = value;
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

    public string GoldStatsApiUrl
    {
        get => _goldStatsApiUrl;
        set
        {
            _goldStatsApiUrl = value;
            OnPropertyChanged();
        }
    }

    public bool IsLootLoggerSaveReminderActive
    {
        get => _isLootLoggerSaveReminderActive;
        set
        {
            _isLootLoggerSaveReminderActive = value;
            OnPropertyChanged();
        }
    }

    public bool ShortDamageMeterToClipboard
    {
        get => _shortDamageMeterToClipboard;
        set
        {
            _shortDamageMeterToClipboard = value;
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

    public string ToolDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion Bindings
}