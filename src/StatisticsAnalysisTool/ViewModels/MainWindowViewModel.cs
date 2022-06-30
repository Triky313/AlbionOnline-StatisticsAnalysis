using FontAwesome5;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using log4net;
using Microsoft.Win32;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Properties;
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

namespace StatisticsAnalysisTool.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static MainWindow _mainWindow;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private static PlayerModeInformationModel _playerModeInformationLocal;
        private static PlayerModeInformationModel _playerModeInformation;
        private double _allianceInfoWidth;
        private double _currentMapInfoWidth;
        private ObservableCollection<DamageMeterFragment> _damageMeter = new();
        private List<DamageMeterSortStruct> _damageMeterSort = new();
        private DamageMeterSortStruct _damageMeterSortSelection;
        private string _errorBarText;
        private Visibility _errorBarVisibility;
        private double _guildInfoWidth;
        private Visibility _isDamageMeterPopupVisible = Visibility.Hidden;
        private bool _isDamageMeterResetByMapChangeActive;
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
        private Visibility _loadIconVisibility;
        private string _loadTranslation;
        private int _localImageCounter;
        private string _numberOfValuesTranslation;
        private ObservableCollection<PartyMemberCircle> _partyMemberCircles = new();
        private PlayerModeTranslation _playerModeTranslation = new();
        private string _savedPlayerInformationName;
        private string _searchText;
        private ShopSubCategory _selectedItemShopSubCategories;
        private ItemLevel _selectedItemLevel;
        private ShopCategory _selectedItemShopCategories;
        private ItemTier _selectedItemTier;
        public TrackingController TrackingController;
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
        private Visibility _gridTryToLoadTheItemListAgainVisibility;
        private EFontAwesomeIcon _damageMeterActivationToggleIcon = EFontAwesomeIcon.Solid_ToggleOff;
        private Brush _damageMeterActivationToggleColor;
        private bool _isDamageMeterTrackingActive;
        private bool _isTrackingPartyLootOnly;
        private Axis[] _xAxesDashboardHourValues;
        private ObservableCollection<ISeries> _seriesDashboardHourValues;
        private DashboardObject _dashboardObject = new();
        private string _loggingSearchText;
        private Visibility _gridTryToLoadTheItemJsonAgainVisibility;
        private Visibility _toolTasksVisibility = Visibility.Collapsed;
        private ObservableCollection<TaskTextObject> _toolTaskObjects = new();
        private double _taskProgressbarMinimum;
        private double _taskProgressbarMaximum = 100;
        private double _taskProgressbarValue;
        private bool _isTaskProgressbarIndeterminate;
        private ObservableCollection<ClusterInfo> _enteredCluster = new();
        private VaultBindings _vaultBindings = new();
        private UserTrackingBindings _userTrackingBindings = new();
        private Visibility _debugModeVisibility = Visibility.Collapsed;
        private TrackingActivityBindings _trackingActivityBindings = new();
        private MailMonitoringBindings _mailMonitoringBindings = new();
        private DungeonBindings _dungeonBindings = new();
        private Visibility _unsupportedOsVisibility = Visibility.Collapsed;
        private LoggingBindings _loggingBindings = new();

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            SettingsController.LoadSettings();
            UpgradeSettings();
            InitWindowSettings();
            AutoUpdateController.AutoUpdate();

            if (!LanguageController.InitializeLanguage())
            {
                _mainWindow.Close();
            }

            _ = InitMainWindowDataAsync().ConfigureAwait(false);
            _ = InitTrackingAsync().ConfigureAwait(false);
        }

        public void SetUiElements()
        {
            #region Error bar

            ErrorBarVisibility = Visibility.Hidden;

            #endregion

            #region Full Item Info elements

            ItemCategories = CategoryController.CategoryNames;
            SelectedItemShopCategory = ShopCategory.Unknown;

            ItemTiers = FrequentlyValues.ItemTiers;
            SelectedItemTier = ItemTier.Unknown;

            ItemLevels = FrequentlyValues.ItemLevels;
            SelectedItemLevel = ItemLevel.Unknown;

            #endregion Full Item Info elements

            #region Player information

            SavedPlayerInformationName = SettingsController.CurrentSettings.SavedPlayerInformationName;

            #endregion Player information

            #region Tracking

            UserTrackingBindings.UsernameInformationVisibility = Visibility.Hidden;
            UserTrackingBindings.GuildInformationVisibility = Visibility.Hidden;
            UserTrackingBindings.AllianceInformationVisibility = Visibility.Hidden;
            UserTrackingBindings.CurrentMapInformationVisibility = Visibility.Hidden;

            IsTrackingResetByMapChangeActive = SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive;

            // Damage meter
            var sortByDamageStruct = new DamageMeterSortStruct
            {
                Name = MainWindowTranslation.SortByDamage,
                DamageMeterSortType = DamageMeterSortType.Damage
            };
            var sortByDpsStruct = new DamageMeterSortStruct
            {
                Name = MainWindowTranslation.SortByDps,
                DamageMeterSortType = DamageMeterSortType.Dps
            };
            var sortByNameStruct = new DamageMeterSortStruct
            {
                Name = MainWindowTranslation.SortByName,
                DamageMeterSortType = DamageMeterSortType.Name
            };
            var sortByHealStruct = new DamageMeterSortStruct
            {
                Name = MainWindowTranslation.SortByHeal,
                DamageMeterSortType = DamageMeterSortType.Heal
            };
            var sortByHpsStruct = new DamageMeterSortStruct
            {
                Name = MainWindowTranslation.SortByHps,
                DamageMeterSortType = DamageMeterSortType.Hps
            };

            DamageMeterSort.Clear();
            DamageMeterSort.Add(sortByDamageStruct);
            DamageMeterSort.Add(sortByDpsStruct);
            DamageMeterSort.Add(sortByNameStruct);
            DamageMeterSort.Add(sortByHealStruct);
            DamageMeterSort.Add(sortByHpsStruct);
            DamageMeterSortSelection = sortByDamageStruct;

            // Dungeons
            DungeonBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.DungeonsGridSplitterPosition);

            // Mail Monitoring
            MailMonitoringBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition);

            // Vault
            VaultBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.StorageHistoryGridSplitterPosition);

            #endregion
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
                var imageAwesome = (ImageAwesome)sender;
                var item = (Item)imageAwesome.DataContext;

                if (item.AlertModeMinSellPriceIsUndercutPrice <= 0)
                {
                    return;
                }

                item.IsAlertActive = AlertManager.ToggleAlert(ref item);
                ItemsView.Refresh();
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

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Fatal(nameof(OnUnhandledException), (Exception)e.ExceptionObject);
            }
            catch (Exception ex)
            {
                Log.Fatal(nameof(OnUnhandledException), ex);
            }
        }
        
        private void InitAlerts()
        {
            SoundController.InitializeSoundFilesFromDirectory();
            AlertManager = new AlertController(_mainWindow, ItemsView);
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

        private static void InitWindowSettings()
        {
            #region Set MainWindow height and width and center window

            _mainWindow.Dispatcher?.Invoke(() =>
            {
                _mainWindow.Height = SettingsController.CurrentSettings.MainWindowHeight;
                _mainWindow.Width = SettingsController.CurrentSettings.MainWindowWidth;
                if (SettingsController.CurrentSettings.MainWindowMaximized)
                {
                    _mainWindow.WindowState = WindowState.Maximized;
                }

                Utilities.CenterWindowOnScreen(_mainWindow);
            });

            #endregion Set MainWindow height and width and center window
        }

        private async Task InitMainWindowDataAsync()
        {
#if DEBUG
            DebugModeVisibility = Visibility.Visible;
#endif
            
            Translation = new MainWindowTranslation();
            ToolTaskController.SetToolTaskController(this);
            SetUiElements();
            UnsupportedOsVisibility = Environment.OSVersion.Version.Major < 10 ? Visibility.Visible : Visibility.Collapsed;

            // TODO: Info window temporarily disabled
            //ShowInfoWindow();

            await InitItemsAsync().ConfigureAwait(false);
        }

        public async Task InitItemsAsync()
        {
            IsTaskProgressbarIndeterminate = true;
            IsTxtSearchEnabled = false;
            IsItemSearchCheckboxesEnabled = false;
            IsFilterResetEnabled = false;
            LoadIconVisibility = Visibility.Visible;
            GridTryToLoadTheItemListAgainVisibility = Visibility.Collapsed;
            GridTryToLoadTheItemJsonAgainVisibility = Visibility.Collapsed;

            if (!ItemController.IsItemsLoaded())
            {
                var itemListTaskTextObject = new TaskTextObject(LanguageController.Translation("GET_ITEM_LIST_JSON"));
                ToolTaskController.Add(itemListTaskTextObject);
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
                ToolTaskController.Add(itemsTaskTextObject);
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

            await ItemController.SetFavoriteItemsFromLocalFileAsync();

            ItemsView = new ListCollectionView(ItemController.Items);
            InitAlerts();

            LoadIconVisibility = Visibility.Hidden;
            IsFilterResetEnabled = true;
            IsItemSearchCheckboxesEnabled = true;
            IsTxtSearchEnabled = true;
            IsTaskProgressbarIndeterminate = false;

            //_mainWindow.Dispatcher?.Invoke(() => { _ = _mainWindow.TxtSearch.Focus(); });
        }

        private async Task InitTrackingAsync()
        {
            await WorldData.GetDataListFromJsonAsync().ConfigureAwait(true);
            await DungeonObjectData.GetDataListFromJsonAsync().ConfigureAwait(true);

            TrackingController ??= new TrackingController(this, _mainWindow);

            StartTracking();

            IsDamageMeterTrackingActive = SettingsController.CurrentSettings.IsDamageMeterTrackingActive;
            IsTrackingPartyLootOnly = SettingsController.CurrentSettings.IsTrackingPartyLootOnly;
            LoggingBindings.IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
            LoggingBindings.IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;
            LoggingBindings.IsTrackingMobLoot = SettingsController.CurrentSettings.IsTrackingMobLoot;

            LoggingBindings.NotificationsCollectionView = CollectionViewSource.GetDefaultView(LoggingBindings.TrackingNotifications) as ListCollectionView;
            if (LoggingBindings?.NotificationsCollectionView != null)
            {
                LoggingBindings.NotificationsCollectionView.IsLiveSorting = true;
                LoggingBindings.NotificationsCollectionView.IsLiveFiltering = true;
                LoggingBindings.NotificationsCollectionView.SortDescriptions.Add(new SortDescription(nameof(DateTime), ListSortDirection.Descending));
            }

            // Logging
            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Fame)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFame,
                Name = MainWindowTranslation.Fame
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Silver)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSilver,
                Name = MainWindowTranslation.Silver
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Faction)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFaction,
                Name = MainWindowTranslation.Faction
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.SeasonPoints)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints,
                Name = MainWindowTranslation.SeasonPoints
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.ConsumableLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot,
                Name = MainWindowTranslation.ConsumableLoot
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.EquipmentLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot,
                Name = MainWindowTranslation.EquipmentLoot
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.SimpleLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot,
                Name = MainWindowTranslation.SimpleLoot
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.UnknownLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot,
                Name = MainWindowTranslation.UnknownLoot
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.ShowLootFromMob)
            {
                IsSelected = SettingsController.CurrentSettings.IsLootFromMobShown,
                Name = MainWindowTranslation.ShowLootFromMobs
            });

            LoggingBindings?.Filters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Kill)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterKill,
                Name = MainWindowTranslation.ShowKills
            });
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

        //private static void ShowInfoWindow()
        //{
        //    if (SettingsController.CurrentSettings.IsInfoWindowShownOnStart)
        //    {
        //        var infoWindow = new InfoWindow();
        //        infoWindow.Show();
        //    }
        //}

        public static void OpenItemWindow(Item item)
        {
            if (string.IsNullOrEmpty(item?.UniqueName))
                return;

            try
            {
                if (!SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked && Utilities.IsWindowOpen<ItemWindow>())
                {
                    var existItemWindow = Application.Current.Windows.OfType<ItemWindow>().FirstOrDefault();
                    existItemWindow?.InitializeItemWindow(item);
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
                    File.WriteAllText(dialog.FileName, TrackingController.LootController.GetLootLoggerObjectsAsCsv(SettingsController.CurrentSettings.IsItemRealNameInLoggingExportActive));
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }

        #endregion

        #region Player information

        public async Task SetComparedPlayerModeInfoValues()
        {
            PlayerModeInformationLocal = PlayerModeInformation;
            PlayerModeInformation = new PlayerModeInformationModel();
            PlayerModeInformation = await GetPlayerModeInformationByApi().ConfigureAwait(true);
        }

        private async Task<PlayerModeInformationModel> GetPlayerModeInformationByApi()
        {
            if (string.IsNullOrWhiteSpace(SavedPlayerInformationName))
                return null;

            var gameInfoSearch = await ApiController.GetGameInfoSearchFromJsonAsync(SavedPlayerInformationName);

            if (gameInfoSearch?.SearchPlayer?.FirstOrDefault()?.Id == null)
                return null;

            var searchPlayer = gameInfoSearch.SearchPlayer?.FirstOrDefault();
            var gameInfoPlayers = await ApiController.GetGameInfoPlayersFromJsonAsync(gameInfoSearch.SearchPlayer?.FirstOrDefault()?.Id);

            return new PlayerModeInformationModel
            {
                Timestamp = DateTime.UtcNow,
                GameInfoSearch = gameInfoSearch,
                SearchPlayer = searchPlayer,
                GameInfoPlayers = gameInfoPlayers
            };
        }

        public PlayerModeInformationModel PlayerModeInformation
        {
            get => _playerModeInformation;
            set
            {
                _playerModeInformation = value;
                OnPropertyChanged();
            }
        }

        public PlayerModeInformationModel PlayerModeInformationLocal
        {
            get => _playerModeInformationLocal;
            set
            {
                _playerModeInformationLocal = value;
                OnPropertyChanged();
            }
        }

        #endregion Player information (Player Mode)

        #region Tracking

        public void StartTracking()
        {
            if (NetworkManager.IsNetworkCaptureRunning)
            {
                return;
            }

            TrackingController?.ClusterController.RegisterEvents();
            TrackingController?.LootController.RegisterEvents();
            TrackingController?.DungeonController?.LoadDungeonFromFile();
            TrackingController?.DungeonController?.SetDungeonStatsDayUi();
            TrackingController?.DungeonController?.SetDungeonStatsTotalUi();
            TrackingController?.DungeonController?.SetOrUpdateDungeonsDataUiAsync();
            TrackingController?.StatisticController?.LoadFromFile();
            TrackingController?.MailController?.LoadFromFile();
            TrackingController?.VaultController?.LoadFromFile();

            TrackingController?.CountUpTimer.Start();

            DungeonBindings.DungeonStatsFilter = new DungeonStatsFilter(TrackingController);

            IsTrackingActive = NetworkManager.StartNetworkCapture(this, TrackingController);
            Console.WriteLine(@"### Start Tracking...");
        }

        public void StopTracking()
        {
            TrackingController?.DungeonController?.SaveDungeonsInFile();
            TrackingController?.StatisticController?.SaveInFile();
            TrackingController?.MailController?.SaveInFile();
            TrackingController?.VaultController?.SaveInFile();
            TrackingController?.LootController.UnregisterEvents();
            TrackingController?.ClusterController.UnregisterEvents();
            TrackingController?.CountUpTimer?.Stop();

            NetworkManager.StopNetworkCapture();

            IsTrackingActive = false;
            Console.WriteLine(@"### Stop Tracking");
        }

        public void ResetDamageMeter()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_DAMAGE_METER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DAMAGE_METER"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                TrackingController.CombatController.ResetDamageMeter();
            }
        }

        public void ResetDungeons()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_DUNGEON_TRACKER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DUNGEON_TRACKER"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                TrackingController.DungeonController.ResetDungeons();
            }
        }

        public async Task ResetTrackingNotificationsAsync()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_TRACKING_NOTIFICATIONS"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                await TrackingController.ClearNotificationsAsync().ConfigureAwait(false);
                Application.Current.Dispatcher.Invoke(() => LoggingBindings.TopLooters.Clear());
                TrackingController.LootController.ClearLootLogger();
            }
        }
        
        public void SetDamageMeterSort()
        {
            switch (DamageMeterSortSelection.DamageMeterSortType)
            {
                case DamageMeterSortType.Damage:
                    SetIsDamageMeterShowing(DamageMeter, true);
                    DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.DamageInPercent).ToList());
                    return;
                case DamageMeterSortType.Dps:
                    SetIsDamageMeterShowing(DamageMeter, true);
                    DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.Dps).ToList());
                    return;
                case DamageMeterSortType.Name:
                    SetIsDamageMeterShowing(DamageMeter, true);
                    DamageMeter.OrderByReference(DamageMeter.OrderBy(x => x.Name).ToList());
                    return;
                case DamageMeterSortType.Heal:
                    SetIsDamageMeterShowing(DamageMeter, false);
                    DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.HealInPercent).ToList());
                    return;
                case DamageMeterSortType.Hps:
                    SetIsDamageMeterShowing(DamageMeter, false);
                    DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.Hps).ToList());
                    break;
            }
        }

        private static void SetIsDamageMeterShowing(IEnumerable<DamageMeterFragment> damageMeter, bool isDamageMeterShowing)
        {
            foreach (var fragment in damageMeter)
            {
                fragment.IsDamageMeterShowing = isDamageMeterShowing;
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
                           && ((ItemTier)item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                           && ((ItemLevel)item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                           && item.IsAlertActive;
                }

                if (IsShowOnlyFavoritesActive)
                {
                    return (item?.LocalizedNameAndEnglish?.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false)
                           && (item.ShopCategory == SelectedItemShopCategory || SelectedItemShopCategory == ShopCategory.Unknown)
                           && (item.ShopShopSubCategory1 == SelectedItemShopSubCategory || SelectedItemShopSubCategory == ShopSubCategory.Unknown)
                           && ((ItemTier)item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                           && ((ItemLevel)item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                           && item.IsFavorite;
                }

                return (item?.LocalizedNameAndEnglish?.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false)
                       && (item.ShopCategory == SelectedItemShopCategory || SelectedItemShopCategory == ShopCategory.Unknown)
                       && (item.ShopShopSubCategory1 == SelectedItemShopSubCategory || SelectedItemShopSubCategory == ShopSubCategory.Unknown)
                       && ((ItemTier)item.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                       && ((ItemLevel)item.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown);
            };

            SetItemCounterAsync();
        }

        private async void SetItemCounterAsync()
        {
            try
            {
                LocalImageCounter = await ImageController.LocalImagesCounterAsync();
                ItemCounterString = $"{((ListCollectionView)ItemsView)?.Count ?? 0}/{ItemController.Items?.Count ?? 0}";
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
                TrackingController?.NotificationUiFilteringAsync(_loggingSearchText);
                OnPropertyChanged();
            }
        }

        public bool IsTrackingPartyLootOnly
        {
            get => _isTrackingPartyLootOnly;
            set
            {
                _isTrackingPartyLootOnly = value;
                TrackingController.LootController.IsPartyLootOnly = _isTrackingPartyLootOnly;

                SettingsController.CurrentSettings.IsTrackingPartyLootOnly = _isTrackingPartyLootOnly;
                OnPropertyChanged();
            }
        }
        
        public DamageMeterSortStruct DamageMeterSortSelection
        {
            get => _damageMeterSortSelection;
            set
            {
                _damageMeterSortSelection = value;
                SetDamageMeterSort();

                OnPropertyChanged();
            }
        }

        public List<DamageMeterSortStruct> DamageMeterSort
        {
            get => _damageMeterSort;
            set
            {
                _damageMeterSort = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DamageMeterFragment> DamageMeter
        {
            get => _damageMeter;
            set
            {
                _damageMeter = value;
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
        
        public UserTrackingBindings UserTrackingBindings
        {
            get => _userTrackingBindings;
            set
            {
                _userTrackingBindings = value;
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

                switch (_isTrackingActive)
                {
                    case true when TrackingController is { ExistIndispensableInfos: false }:
                        TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsPartiallyActive;
                        TrackingActivityBindings.TrackingActivityType = TrackingIconType.Partially;
                        break;
                    case true when TrackingController is { ExistIndispensableInfos: true }:
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
                if (TrackingController?.CombatController == null)
                {
                    return;
                }

                _isDamageMeterTrackingActive = value;

                TrackingController.CombatController.IsDamageMeterActive = _isDamageMeterTrackingActive;

                DamageMeterActivationToggleIcon = _isDamageMeterTrackingActive ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;

                var colorOn = new SolidColorBrush((Color)Application.Current.Resources["Color.Accent.Blue.2"]);
                var colorOff = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.1"]);
                DamageMeterActivationToggleColor = _isDamageMeterTrackingActive ? colorOn : colorOff;

                SettingsController.CurrentSettings.IsDamageMeterTrackingActive = _isDamageMeterTrackingActive;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon DamageMeterActivationToggleIcon
        {
            get => _damageMeterActivationToggleIcon;
            set
            {
                _damageMeterActivationToggleIcon = value;
                OnPropertyChanged();
            }
        }

        public Brush DamageMeterActivationToggleColor
        {
            get => _damageMeterActivationToggleColor ?? new SolidColorBrush((Color)Application.Current.Resources["Color.Text.1"]);
            set
            {
                _damageMeterActivationToggleColor = value;
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

        public bool IsDamageMeterResetByMapChangeActive
        {
            get => _isDamageMeterResetByMapChangeActive;
            set
            {
                _isDamageMeterResetByMapChangeActive = value;
                OnPropertyChanged();
            }
        }

        public DashboardObject DashboardObject
        {
            get => _dashboardObject;
            set
            {
                _dashboardObject = value;
                OnPropertyChanged();
            }
        }

        public PlayerModeTranslation PlayerModeTranslation
        {
            get => _playerModeTranslation;
            set
            {
                _playerModeTranslation = value;
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

        public string SavedPlayerInformationName
        {
            get => _savedPlayerInformationName;
            set
            {
                _savedPlayerInformationName = value;
                SettingsController.CurrentSettings.SavedPlayerInformationName = _savedPlayerInformationName;
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

        public ObservableCollection<TaskTextObject> ToolTaskObjects
        {
            get => _toolTaskObjects;
            set
            {
                _toolTaskObjects = value;
                OnPropertyChanged();
            }
        }

        public MailMonitoringBindings MailMonitoringBindings
        {
            get => _mailMonitoringBindings;
            set
            {
                _mailMonitoringBindings = value;
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
        public static string ItemListJsonHyperlink => "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json";
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

        #region Structs
        
        public struct DamageMeterSortStruct
        {
            public string Name { get; set; }
            public DamageMeterSortType DamageMeterSortType { get; set; }
        }

        #endregion
    }
}