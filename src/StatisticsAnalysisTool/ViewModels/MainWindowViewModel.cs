using FontAwesome5;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using log4net;
using Microsoft.Win32;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

// ReSharper disable UnusedMember.Global

namespace StatisticsAnalysisTool.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private double _allianceInfoWidth;
    private double _currentMapInfoWidth;
    private string _errorBarText;
    private Visibility _errorBarVisibility = Visibility.Collapsed;
    private double _guildInfoWidth;
    private Visibility _isDamageMeterPopupVisible = Visibility.Hidden;
    private bool _isShowOnlyItemsWithAlertOnActive;
    private bool _isTrackingActive;
    private bool _isTrackingResetByMapChangeActive;
    private bool _isTxtSearchEnabled;
    private Dictionary<ShopSubCategory, string> _itemSubCategories = new();
    private string _itemCounterString;
    private Dictionary<ItemLevel, string> _itemLevels = new();
    private Dictionary<ShopCategory, string> _itemCategories = new();
    private ICollectionView _itemsView;
    private Dictionary<ItemTier, string> _itemTiers = new();
    private Visibility _loadIconVisibility = Visibility.Collapsed;
    private string _loadTranslation;
    private int _localImageCounter;
    private string _numberOfValuesTranslation;
    private ObservableCollection<PartyMemberCircle> _partyMemberCircles = new();
    private string _searchText;
    private ShopSubCategory _selectedItemShopSubCategories;
    private ItemLevel _selectedItemLevel;
    private ShopCategory _selectedItemShopCategories;
    private ItemTier _selectedItemTier;
    private MainWindowTranslation _translation;
    private string _updateTranslation;
    private double _usernameInfoWidth;
    public AlertController AlertManager;
    private ObservableCollection<MainStatObject> _factionPointStats = new() { new MainStatObject() { Value = 0, ValuePerHour = 0, CityFaction = CityFaction.Unknown } };
    private string _mainTrackerTimer;
    private bool _isShowOnlyFavoritesActive;
    private int _partyMemberNumber;
    private bool _isItemSearchCheckboxesEnabled;
    private bool _isFilterResetEnabled;
    private Visibility _gridTryToLoadTheItemListAgainVisibility = Visibility.Collapsed;
    private bool _isDamageMeterTrackingActive;
    private bool _isTrackingPartyLootOnly;
    private Axis[] _xAxesDashboardHourValues;
    private ObservableCollection<ISeries> _seriesDashboardHourValues;
    private DashboardBindings _dashboardBindings = new();
    private string _loggingSearchText;
    private Visibility _gridTryToLoadTheItemJsonAgainVisibility = Visibility.Collapsed;
    private Visibility _gridTryToLoadTheMobsJsonAgainVisibility = Visibility.Collapsed;
    private Visibility _toolTasksVisibility = Visibility.Collapsed;
    private double _taskProgressbarMinimum;
    private double _taskProgressbarMaximum = 100;
    private double _taskProgressbarValue;
    private bool _isTaskProgressbarIndeterminate;
    private ObservableCollection<ClusterInfo> _enteredCluster = new();
    private VaultBindings _vaultBindings = new();
    private UserTrackingBindings _userTrackingBindings = new();
    private Visibility _debugModeVisibility = Visibility.Collapsed;
    private TrackingActivityBindings _trackingActivityBindings = new();
    private TradeMonitoringBindings _tradeMonitoringBindings = new();
    private DungeonBindings _dungeonBindings = new();
    private DamageMeterBindings _damageMeterBindings = new();
    private Visibility _unsupportedOsVisibility = Visibility.Collapsed;
    private LoggingBindings _loggingBindings = new();
    private PlayerInformationBindings _playerInformationBindings = new();
    private GatheringBindings _gatheringBindings = new();
    private Visibility _dashboardTabVisibility = Visibility.Visible;
    private Visibility _itemSearchTabVisibility = Visibility.Visible;
    private Visibility _loggingTabVisibility = Visibility.Visible;
    private Visibility _dungeonsTabVisibility = Visibility.Visible;
    private Visibility _damageMeterTabVisibility = Visibility.Visible;
    private Visibility _tradeMonitoringTabVisibility = Visibility.Visible;
    private Visibility _gatheringTabVisibility = Visibility.Visible;
    private Visibility _storageHistoryTabVisibility = Visibility.Visible;
    private Visibility _mapHistoryTabVisibility = Visibility.Visible;
    private Visibility _playerInformationTabVisibility = Visibility.Visible;
    private Visibility _toolTaskFrontViewVisibility = Visibility.Collapsed;
    private double _toolTaskProgressBarValue;
    private string _toolTaskCurrentTaskName;
    private ToolTaskBindings _toolTaskBindings = new();
    private string _serverTypeText;

    public MainWindowViewModel()
    {
        UpgradeSettings();
        SetUiElements();
        Translation = new MainWindowTranslation();
    }

    public void SetUiElements()
    {
        // Error bar
        ErrorBarVisibility = Visibility.Collapsed;

        // Unsupported OS
        UnsupportedOsVisibility = Environment.OSVersion.Version.Major < 10 ? Visibility.Visible : Visibility.Collapsed;

        // Item search
        ItemCategories = CategoryController.CategoryNames;
        SelectedItemShopCategory = ShopCategory.Unknown;

        ItemTiers = FrequentlyValues.ItemTiers;
        SelectedItemTier = ItemTier.Unknown;

        ItemLevels = FrequentlyValues.ItemLevels;
        SelectedItemLevel = ItemLevel.Unknown;

        // Tracking
        UserTrackingBindings.UsernameInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.GuildInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.AllianceInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.CurrentMapInfoBinding.CurrentMapInformationVisibility = Visibility.Hidden;

        IsTrackingResetByMapChangeActive = SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive;

        // Dungeons
        DungeonBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.DungeonsGridSplitterPosition);

        // Mail Monitoring
        TradeMonitoringBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition);

        // Vault
        VaultBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.StorageHistoryGridSplitterPosition);

        // Damage Meter
        DamageMeterBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.DamageMeterGridSplitterPosition);

        // Gathering
        GatheringBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.GatheringGridSplitterPosition);
    }

    #region Alert

    public void ToggleAlertSender(object sender)
    {
        if (sender == null)
        {
            return;
        }

        try
        {
            var imageAwesome = (ImageAwesome) sender;
            var item = (Item) imageAwesome.DataContext;

            if (item.AlertModeMinSellPriceIsUndercutPrice <= 0)
            {
                return;
            }

            item.IsAlertActive = AlertManager.ToggleAlert(ref item);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion

    #region Item list

    public void ItemFilterReset()
    {
        SearchText = string.Empty;
        SelectedItemShopSubCategory = ShopSubCategory.Unknown;
        SelectedItemShopCategory = ShopCategory.Unknown;
        SelectedItemLevel = ItemLevel.Unknown;
        SelectedItemTier = ItemTier.Unknown;
    }

    #endregion Item list

    #region Error bar

    public void SetErrorBar(Visibility visibility, string errorMessage)
    {
        ErrorBarText = errorMessage;
        ErrorBarVisibility = visibility;
    }

    #endregion

    #region Inits

    private void InitAlerts()
    {
        SoundController.InitializeSoundFilesFromDirectory();
        AlertManager = new AlertController(ItemsView);
    }

    private static void UpgradeSettings()
    {
        if (!Settings.Default.UpgradeRequired)
        {
            return;
        }

        Settings.Default.Upgrade();
        Settings.Default.UpgradeRequired = false;
        Settings.Default.Save();
    }

    public void InitMainWindowData()
    {
#if DEBUG
        DebugModeVisibility = Visibility.Visible;
#endif

        _ = InitAsync();
    }

    public async Task InitAsync()
    {
        IsTaskProgressbarIndeterminate = true;
        IsTxtSearchEnabled = false;
        IsItemSearchCheckboxesEnabled = false;
        IsFilterResetEnabled = false;
        GridTryToLoadTheItemListAgainVisibility = Visibility.Collapsed;
        GridTryToLoadTheItemJsonAgainVisibility = Visibility.Collapsed;
        GridTryToLoadTheMobsJsonAgainVisibility = Visibility.Collapsed;

        ServerTypeText = LanguageController.Translation("UNKNOWN_SERVER");

        if (!ItemController.IsItemsLoaded())
        {
            var itemListTaskTextObject = new TaskTextObject(LanguageController.Translation("GET_ITEM_LIST_JSON"));
            ToolTaskBindings.Add(itemListTaskTextObject);
            var isItemListLoaded = await ItemController.GetItemListFromJsonAsync().ConfigureAwait(true);
            if (!isItemListLoaded)
            {
                SetErrorBar(Visibility.Visible, LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"));
                GridTryToLoadTheItemListAgainVisibility = Visibility.Visible;
                IsTaskProgressbarIndeterminate = false;
                itemListTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Canceled);
            }
            else
            {
                itemListTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Done);
            }
        }

        if (!ItemController.IsItemsJsonLoaded())
        {
            var itemsTaskTextObject = new TaskTextObject(LanguageController.Translation("GET_ITEMS_JSON"));
            ToolTaskBindings.Add(itemsTaskTextObject);
            var isItemsJsonLoaded = await ItemController.GetItemsJsonAsync().ConfigureAwait(true);
            if (!isItemsJsonLoaded)
            {
                SetErrorBar(Visibility.Visible, LanguageController.Translation("ITEM_JSON_CAN_NOT_BE_LOADED"));
                GridTryToLoadTheItemJsonAgainVisibility = Visibility.Visible;
                IsTaskProgressbarIndeterminate = false;
                itemsTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Canceled);
            }
            else
            {
                itemsTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Done);
            }
        }

        if (!MobsData.IsDataLoaded())
        {
            var itemsTaskTextObject = new TaskTextObject(LanguageController.Translation("GET_MOBS_JSON"));
            ToolTaskBindings.Add(itemsTaskTextObject);
            var isMobsJsonLoaded = await MobsData.LoadMobsDataAsync().ConfigureAwait(true);
            if (!isMobsJsonLoaded)
            {
                SetErrorBar(Visibility.Visible, LanguageController.Translation("MOBS_JSON_CAN_NOT_BE_LOADED"));
                GridTryToLoadTheMobsJsonAgainVisibility = Visibility.Visible;
                IsTaskProgressbarIndeterminate = false;
                itemsTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Canceled);
            }
            else
            {
                itemsTaskTextObject.SetStatus(TaskTextObject.TaskTextObjectStatus.Done);
            }
        }

        await ItemController.SetFavoriteItemsFromLocalFileAsync();

        ItemsView = new ListCollectionView(ItemController.Items);
        InitAlerts();
        await EstimatedMarketValueController.SetAllEstimatedMarketValuesToItemsAsync();
        LoggingBindings.Init();

        LoadIconVisibility = Visibility.Hidden;
        IsFilterResetEnabled = true;
        IsItemSearchCheckboxesEnabled = true;
        IsTxtSearchEnabled = true;
        IsTaskProgressbarIndeterminate = false;
    }

    #endregion

    #region Tool tasks

    public void SetToolTasksVisibility(Visibility value)
    {
        ToolTasksVisibility = value;
    }

    public void SwitchToolTasksState()
    {
        ToolTasksVisibility = ToolTasksVisibility switch
        {
            Visibility.Collapsed => Visibility.Visible,
            Visibility.Visible => Visibility.Collapsed,
            _ => ToolTasksVisibility
        };
    }

    #endregion

    #region Save loot logger

    public void SaveLootLogger()
    {
        if (!SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive)
        {
            return;
        }

        try
        {
            var dialog = new DialogWindow(LanguageController.Translation("SAVE_LOOT_LOGGER"), LanguageController.Translation("SAVE_LOOT_LOGGER_NOW"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                ExportLootToFile();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion

    #region Ui utility methods

    public static void OpenItemWindow(Item item)
    {
        if (string.IsNullOrEmpty(item?.UniqueName))
            return;

        try
        {
            if (!SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked && Utilities.IsWindowOpen<ItemWindow>())
            {
                var existItemWindow = Application.Current.Windows.OfType<ItemWindow>().FirstOrDefault();
                //existItemWindow?.Init(item);
                existItemWindow?.Activate();
            }
            else
            {
                var itemWindow = new ItemWindow(item);
                itemWindow.Show();
            }
        }
        catch (ArgumentNullException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            var catchItemWindow = new ItemWindow(item);
            catchItemWindow.Show();
        }
    }

    public void ExportLootToFile()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"log-{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}utc",
            DefaultExt = ".csv",
            Filter = "CSV documents (.csv)|*.csv"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var trackingController = ServiceLocator.Resolve<TrackingController>();
                File.WriteAllText(dialog.FileName, trackingController?.LootController?.GetLootLoggerObjectsAsCsv());
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }

    #endregion

    #region Tracking

    // TODO: Move in tracking controllers
    public void ResetDamageMeter()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_DAMAGE_METER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DAMAGE_METER"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController?.CombatController?.ResetDamageMeter();
        }
    }

    public void ResetDungeons()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_DUNGEON_TRACKER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DUNGEON_TRACKER"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController?.DungeonController?.ResetDungeons();
        }
    }

    public async Task ResetTrackingNotificationsAsync()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_TRACKING_NOTIFICATIONS"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            await trackingController.ClearNotificationsAsync();
            Application.Current.Dispatcher.Invoke(() => LoggingBindings.TopLooters.Clear());
            trackingController.LootController?.ClearLootLogger();
        }
    }

    public void DamageMeterActivationToggle()
    {
        IsDamageMeterTrackingActive = !IsDamageMeterTrackingActive;
    }

    #endregion

    #region Item View Filters

    private void ItemsViewFilter()
    {
        if (ItemsView == null)
        {
            return;
        }

        ItemsView.Filter = i =>
        {
            var item = i as Item;
            if (IsShowOnlyItemsWithAlertOnActive)
            {
                return (item?.LocalizedNameAndEnglish?.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false)
                       && (item.ShopCategory == SelectedItemShopCategory || SelectedItemShopCategory == ShopCategory.Unknown)
                       && (item.ShopShopSubCategory1 == SelectedItemShopSubCategory || SelectedItemShopSubCategory == ShopSubCategory.Unknown)
                       && ((ItemTier) item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                       && ((ItemLevel) item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                       && item.IsAlertActive;
            }

            if (IsShowOnlyFavoritesActive)
            {
                return (item?.LocalizedNameAndEnglish?.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false)
                       && (item.ShopCategory == SelectedItemShopCategory || SelectedItemShopCategory == ShopCategory.Unknown)
                       && (item.ShopShopSubCategory1 == SelectedItemShopSubCategory || SelectedItemShopSubCategory == ShopSubCategory.Unknown)
                       && ((ItemTier) item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                       && ((ItemLevel) item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                       && item.IsFavorite;
            }

            return (item?.LocalizedNameAndEnglish?.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false)
                   && (item.ShopCategory == SelectedItemShopCategory || SelectedItemShopCategory == ShopCategory.Unknown)
                   && (item.ShopShopSubCategory1 == SelectedItemShopSubCategory || SelectedItemShopSubCategory == ShopSubCategory.Unknown)
                   && ((ItemTier) item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                   && ((ItemLevel) item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown);
        };

        SetItemCounterAsync();
    }

    private async void SetItemCounterAsync()
    {
        try
        {
            LocalImageCounter = await ImageController.LocalImagesCounterAsync();
            ItemCounterString = $"{((ListCollectionView) ItemsView)?.Count ?? 0}/{ItemController.Items?.Count ?? 0}";
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion

    #region Bindings

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;

            ItemsViewFilter();
            ItemsView?.Refresh();

            OnPropertyChanged();
        }
    }

    public ICollectionView ItemsView
    {
        get => _itemsView;
        set
        {
            _itemsView = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsDamageMeterPopupVisible
    {
        get => _isDamageMeterPopupVisible;
        set
        {
            _isDamageMeterPopupVisible = value;
            OnPropertyChanged();
        }
    }

    public string LoggingSearchText
    {
        get => _loggingSearchText;
        set
        {
            _loggingSearchText = value;
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController?.NotificationUiFilteringAsync(_loggingSearchText);
            OnPropertyChanged();
        }
    }

    public bool IsTrackingPartyLootOnly
    {
        get => _isTrackingPartyLootOnly;
        set
        {
            _isTrackingPartyLootOnly = value;
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController.LootController.IsPartyLootOnly = _isTrackingPartyLootOnly;

            SettingsController.CurrentSettings.IsTrackingPartyLootOnly = _isTrackingPartyLootOnly;
            OnPropertyChanged();
        }
    }

    public DamageMeterBindings DamageMeterBindings
    {
        get => _damageMeterBindings;
        set
        {
            _damageMeterBindings = value;
            OnPropertyChanged();
        }
    }

    public DungeonBindings DungeonBindings
    {
        get => _dungeonBindings;
        set
        {
            _dungeonBindings = value;
            OnPropertyChanged();
        }
    }

    public GatheringBindings GatheringBindings
    {
        get => _gatheringBindings;
        set
        {
            _gatheringBindings = value;
            OnPropertyChanged();
        }
    }

    public UserTrackingBindings UserTrackingBindings
    {
        get => _userTrackingBindings;
        set
        {
            _userTrackingBindings = value;
            OnPropertyChanged();
        }
    }

    public PlayerInformationBindings PlayerInformationBindings
    {
        get => _playerInformationBindings;
        set
        {
            _playerInformationBindings = value;
            OnPropertyChanged();
        }
    }

    public double UsernameInfoWidth
    {
        get => _usernameInfoWidth;
        set
        {
            _usernameInfoWidth = value;
            OnPropertyChanged();
        }
    }

    public double GuildInfoWidth
    {
        get => _guildInfoWidth;
        set
        {
            _guildInfoWidth = value;
            OnPropertyChanged();
        }
    }

    public double AllianceInfoWidth
    {
        get => _allianceInfoWidth;
        set
        {
            _allianceInfoWidth = value;
            OnPropertyChanged();
        }
    }

    public double CurrentMapInfoWidth
    {
        get => _currentMapInfoWidth;
        set
        {
            _currentMapInfoWidth = value;
            OnPropertyChanged();
        }
    }

    public string MainTrackerTimer
    {
        get => _mainTrackerTimer;
        set
        {
            _mainTrackerTimer = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingResetByMapChangeActive
    {
        get => _isTrackingResetByMapChangeActive;
        set
        {
            _isTrackingResetByMapChangeActive = value;
            SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive = _isTrackingResetByMapChangeActive;
            OnPropertyChanged();
        }
    }

    public LoggingBindings LoggingBindings
    {
        get => _loggingBindings;
        set
        {
            _loggingBindings = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingActive
    {
        get => _isTrackingActive;
        set
        {
            _isTrackingActive = value;
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            switch (_isTrackingActive)
            {
                case true when trackingController is { ExistIndispensableInfos: false }:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsPartiallyActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.Partially;
                    break;
                case true when trackingController is { ExistIndispensableInfos: true }:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.On;
                    break;
                case false:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsNotActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.Off;
                    break;
            }

            OnPropertyChanged();
        }
    }

    public TrackingActivityBindings TrackingActivityBindings
    {
        get => _trackingActivityBindings;
        set
        {
            _trackingActivityBindings = value;
            OnPropertyChanged();
        }
    }

    public bool IsDamageMeterTrackingActive
    {
        get => _isDamageMeterTrackingActive;
        set
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            if (trackingController?.CombatController == null)
            {
                return;
            }

            _isDamageMeterTrackingActive = value;

            trackingController.CombatController.IsDamageMeterActive = _isDamageMeterTrackingActive;

            DamageMeterBindings.DamageMeterActivationToggleIcon = _isDamageMeterTrackingActive ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;

            var colorOn = new SolidColorBrush((Color) Application.Current.Resources["Color.Accent.Blue.2"]);
            var colorOff = new SolidColorBrush((Color) Application.Current.Resources["Color.Text.1"]);
            DamageMeterBindings.DamageMeterActivationToggleColor = _isDamageMeterTrackingActive ? colorOn : colorOff;

            SettingsController.CurrentSettings.IsDamageMeterTrackingActive = _isDamageMeterTrackingActive;
            OnPropertyChanged();
        }
    }

    public bool IsShowOnlyItemsWithAlertOnActive
    {
        get => _isShowOnlyItemsWithAlertOnActive;
        set
        {
            _isShowOnlyItemsWithAlertOnActive = value;

            if (value)
            {
                IsShowOnlyFavoritesActive = false;
            }

            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public bool IsShowOnlyFavoritesActive
    {
        get => _isShowOnlyFavoritesActive;
        set
        {
            _isShowOnlyFavoritesActive = value;

            if (value)
            {
                IsShowOnlyItemsWithAlertOnActive = false;
            }

            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public Visibility LoadIconVisibility
    {
        get => _loadIconVisibility;
        set
        {
            _loadIconVisibility = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PartyMemberCircle> PartyMemberCircles
    {
        get => _partyMemberCircles;
        set
        {
            _partyMemberCircles = value;
            OnPropertyChanged();
        }
    }

    public int PartyMemberNumber
    {
        get => _partyMemberNumber;
        set
        {
            _partyMemberNumber = value;
            OnPropertyChanged();
        }
    }

    public Dictionary<ShopSubCategory, string> ItemSubCategories
    {
        get => _itemSubCategories;
        set
        {
            var categories = value;
            categories = new Dictionary<ShopSubCategory, string> { { ShopSubCategory.Unknown, string.Empty } }.Concat(categories)
                .ToDictionary(k => k.Key, v => v.Value);
            _itemSubCategories = categories;
            OnPropertyChanged();
        }
    }

    public ShopSubCategory SelectedItemShopSubCategory
    {
        get => _selectedItemShopSubCategories;
        set
        {
            _selectedItemShopSubCategories = value;
            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public Dictionary<ShopCategory, string> ItemCategories
    {
        get => _itemCategories;
        set
        {
            _itemCategories = value;
            OnPropertyChanged();
        }
    }

    public ShopCategory SelectedItemShopCategory
    {
        get => _selectedItemShopCategories;
        set
        {
            _selectedItemShopCategories = value;
            ItemSubCategories = CategoryController.GetSubCategoriesByCategory(SelectedItemShopCategory);
            SelectedItemShopSubCategory = ShopSubCategory.Unknown;
            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public Dictionary<ItemTier, string> ItemTiers
    {
        get => _itemTiers;
        set
        {
            _itemTiers = value;
            OnPropertyChanged();
        }
    }

    public ItemTier SelectedItemTier
    {
        get => _selectedItemTier;
        set
        {
            _selectedItemTier = value;
            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public Dictionary<ItemLevel, string> ItemLevels
    {
        get => _itemLevels;
        set
        {
            _itemLevels = value;
            OnPropertyChanged();
        }
    }

    public ItemLevel SelectedItemLevel
    {
        get => _selectedItemLevel;
        set
        {
            _selectedItemLevel = value;
            ItemsView?.Refresh();
            SetItemCounterAsync();
            OnPropertyChanged();
        }
    }

    public int LocalImageCounter
    {
        get => _localImageCounter;
        set
        {
            _localImageCounter = value;
            OnPropertyChanged();
        }
    }

    public string ItemCounterString
    {
        get => _itemCounterString;
        set
        {
            _itemCounterString = value;
            OnPropertyChanged();
        }
    }

    public bool IsTxtSearchEnabled
    {
        get => _isTxtSearchEnabled;
        set
        {
            _isTxtSearchEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsItemSearchCheckboxesEnabled
    {
        get => _isItemSearchCheckboxesEnabled;
        set
        {
            _isItemSearchCheckboxesEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsFilterResetEnabled
    {
        get => _isFilterResetEnabled;
        set
        {
            _isFilterResetEnabled = value;
            OnPropertyChanged();
        }
    }

    public DashboardBindings DashboardBindings
    {
        get => _dashboardBindings;
        set
        {
            _dashboardBindings = value;
            OnPropertyChanged();
        }
    }

    public ToolTaskBindings ToolTaskBindings
    {
        get => _toolTaskBindings;
        set
        {
            _toolTaskBindings = value;
            OnPropertyChanged();
        }
    }

    public string LoadTranslation
    {
        get => _loadTranslation;
        set
        {
            _loadTranslation = value;
            OnPropertyChanged();
        }
    }

    public string NumberOfValuesTranslation
    {
        get => _numberOfValuesTranslation;
        set
        {
            _numberOfValuesTranslation = value;
            OnPropertyChanged();
        }
    }

    public Visibility DebugModeVisibility
    {
        get => _debugModeVisibility;
        set
        {
            _debugModeVisibility = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MainStatObject> FactionPointStats
    {
        get => _factionPointStats;
        set
        {
            _factionPointStats = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ISeries> SeriesDashboardHourValues
    {
        get => _seriesDashboardHourValues;
        set
        {
            _seriesDashboardHourValues = value;
            OnPropertyChanged();
        }
    }

    public Axis[] XAxesDashboardHourValues
    {
        get => _xAxesDashboardHourValues;
        set
        {
            _xAxesDashboardHourValues = value;
            OnPropertyChanged();
        }
    }

    public Visibility GridTryToLoadTheItemListAgainVisibility
    {
        get => _gridTryToLoadTheItemListAgainVisibility;
        set
        {
            _gridTryToLoadTheItemListAgainVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility GridTryToLoadTheItemJsonAgainVisibility
    {
        get => _gridTryToLoadTheItemJsonAgainVisibility;
        set
        {
            _gridTryToLoadTheItemJsonAgainVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility GridTryToLoadTheMobsJsonAgainVisibility
    {
        get => _gridTryToLoadTheMobsJsonAgainVisibility;
        set
        {
            _gridTryToLoadTheMobsJsonAgainVisibility = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMinimum
    {
        get => _taskProgressbarMinimum;
        set
        {
            _taskProgressbarMinimum = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMaximum
    {
        get => _taskProgressbarMaximum;
        set
        {
            _taskProgressbarMaximum = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarValue
    {
        get => _taskProgressbarValue;
        set
        {
            _taskProgressbarValue = value;
            OnPropertyChanged();
        }
    }

    public bool IsTaskProgressbarIndeterminate
    {
        get => _isTaskProgressbarIndeterminate;
        set
        {
            _isTaskProgressbarIndeterminate = value;
            OnPropertyChanged();
        }
    }

    public Visibility ToolTasksVisibility
    {
        get => _toolTasksVisibility;
        set
        {
            _toolTasksVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility UnsupportedOsVisibility
    {
        get => _unsupportedOsVisibility;
        set
        {
            _unsupportedOsVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility DashboardTabVisibility
    {
        get => _dashboardTabVisibility;
        set
        {
            _dashboardTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ItemSearchTabVisibility
    {
        get => _itemSearchTabVisibility;
        set
        {
            _itemSearchTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility LoggingTabVisibility
    {
        get => _loggingTabVisibility;
        set
        {
            _loggingTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility DungeonsTabVisibility
    {
        get => _dungeonsTabVisibility;
        set
        {
            _dungeonsTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility DamageMeterTabVisibility
    {
        get => _damageMeterTabVisibility;
        set
        {
            _damageMeterTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility TradeMonitoringTabVisibility
    {
        get => _tradeMonitoringTabVisibility;
        set
        {
            _tradeMonitoringTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility GatheringTabVisibility
    {
        get => _gatheringTabVisibility;
        set
        {
            _gatheringTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility StorageHistoryTabVisibility
    {
        get => _storageHistoryTabVisibility;
        set
        {
            _storageHistoryTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility MapHistoryTabVisibility
    {
        get => _mapHistoryTabVisibility;
        set
        {
            _mapHistoryTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility PlayerInformationTabVisibility
    {
        get => _playerInformationTabVisibility;
        set
        {
            _playerInformationTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public double ToolTaskProgressBarValue
    {
        get => _toolTaskProgressBarValue;
        set
        {
            _toolTaskProgressBarValue = value;
            OnPropertyChanged();
        }
    }

    public string ToolTaskCurrentTaskName
    {
        get => _toolTaskCurrentTaskName;
        set
        {
            _toolTaskCurrentTaskName = value;
            OnPropertyChanged();
        }
    }

    public Visibility ToolTaskFrontViewVisibility
    {
        get => _toolTaskFrontViewVisibility;
        set
        {
            _toolTaskFrontViewVisibility = value;
            OnPropertyChanged();
        }
    }

    public TradeMonitoringBindings TradeMonitoringBindings
    {
        get => _tradeMonitoringBindings;
        set
        {
            _tradeMonitoringBindings = value;
            OnPropertyChanged();
        }
    }

    public VaultBindings VaultBindings
    {
        get => _vaultBindings;
        set
        {
            _vaultBindings = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ClusterInfo> EnteredCluster
    {
        get => _enteredCluster;
        set
        {
            _enteredCluster = value;
            OnPropertyChanged();
        }
    }

    public Visibility ErrorBarVisibility
    {
        get => _errorBarVisibility;
        set
        {
            _errorBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public string ServerTypeText
    {
        get => _serverTypeText;
        set
        {
            _serverTypeText = value;
            OnPropertyChanged();
        }
    }

    public string ErrorBarText
    {
        get => _errorBarText;
        set
        {
            _errorBarText = value;
            OnPropertyChanged();
        }
    }

    public string UpdateTranslation
    {
        get => _updateTranslation;
        set
        {
            _updateTranslation = value;
            OnPropertyChanged();
        }
    }

    public MainWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    public static string LootLoggerViewer => "https://matheus.sampaio.us/ao-loot-logger-viewer/";
    public static string ItemListJsonHyperlink => "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/formatted/items.json";
    public static string ItemsJsonHyperlink => "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/items.json";

    public static string ToolDirectory => AppDomain.CurrentDomain.BaseDirectory;
    public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion Bindings
}