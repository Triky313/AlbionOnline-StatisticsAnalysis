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
using StatisticsAnalysisTool.Models.NetworkModel;
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
        private Visibility _allianceInformationVisibility;
        private double _allianceInfoWidth;
        private Visibility _currentMapInformationVisibility;
        private double _currentMapInfoWidth;
        private ObservableCollection<DamageMeterFragment> _damageMeter = new();
        private List<DamageMeterSortStruct> _damageMeterSort = new();
        private DamageMeterSortStruct _damageMeterSortSelection;
        private DungeonStats _dungeonStatsDay = new();
        private DungeonStats _dungeonStatsTotal = new();
        private string _errorBarText;
        private Visibility _errorBarVisibility;
        private Visibility _guildInformationVisibility;
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
        private string _trackingAllianceName;
        public TrackingController TrackingController;
        private string _trackingCurrentMapName;
        private ObservableCollection<DungeonNotificationFragment> _trackingDungeons = new();
        private string _trackingGuildName;
        private ObservableCollection<TrackingNotification> _trackingNotifications = new();
        private string _trackingUsername;
        private MainWindowTranslation _translation;
        private string _updateTranslation;
        private Visibility _usernameInformationVisibility;
        private double _usernameInfoWidth;
        public AlertController AlertManager;
        private ObservableCollection<MainStatObject> _factionPointStats = new() { new MainStatObject() { Value = 0, ValuePerHour = 0, CityFaction = CityFaction.Unknown } };
        private string _mainTrackerTimer;
        private bool _isShowOnlyFavoritesActive;
        private DungeonCloseTimer _dungeonCloseTimer;
        private EFontAwesomeIcon _dungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleDown;
        private double _dungeonStatsGridHeight = 82;
        private Thickness _dungeonStatsScrollViewerMargin = new(0, 82, 0, 0);
        private bool _isDungeonStatsGridUnfold;
        private DungeonStatsFilter _dungeonStatsFilter;
        private TrackingIconType _trackingActivityColor;
        private int _partyMemberNumber;
        private ObservableCollection<ClusterInfo> _enteredCluster = new();
        private bool _isItemSearchCheckboxesEnabled;
        private bool _isFilterResetEnabled;
        private Visibility _gridTryToLoadTheItemListAgainVisibility;
        private EFontAwesomeIcon _damageMeterActivationToggleIcon = EFontAwesomeIcon.Solid_ToggleOff;
        private Brush _damageMeterActivationToggleColor;
        private bool _isDamageMeterTrackingActive;
        private ListCollectionView _trackingDungeonsCollectionView;
        private ListCollectionView _trackingNotificationsCollectionView;
        private bool _isTrackingPartyLootOnly;
        private bool _isTrackingSilver;
        private bool _isTrackingFame;
        private string _trackingActiveText = MainWindowTranslation.TrackingIsNotActive;
        private Axis[] _xAxesDashboardHourValues;
        private ObservableCollection<ISeries> _seriesDashboardHourValues;
        private DashboardObject _dashboardObject = new();
        private string _loggingSearchText;
        private ObservableCollection<LoggingFilterObject> _loggingFilters = new();
        private bool _isTrackingMobLoot;
        private Visibility _gridTryToLoadTheItemJsonAgainVisibility;
        private ObservableCollection<TopLooterObject> _topLooters = new();
        private Visibility _toolTasksVisibility = Visibility.Collapsed;
        private ObservableCollection<TaskTextObject> _toolTaskObjects = new();
        private double _taskProgressbarMinimum;
        private double _taskProgressbarMaximum = 100;
        private double _taskProgressbarValue;
        private bool _isTaskProgressbarIndeterminate;
        private Visibility _characterIsNotTrackedInfoVisibility;
        private Visibility _isMailMonitoringPopupVisible = Visibility.Hidden;
        private ObservableCollectionEx<Mail> _mails = new();
        private MailStatsObject _mailStatsObject = new ();
        private ListCollectionView _mailCollectionView;
        private string _mailsSearchText;
        private DateTime _datePickerMailsFrom = new (2017, 1, 1);
        private DateTime _datePickerMailsTo = DateTime.UtcNow.AddDays(1);

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

            UsernameInformationVisibility = Visibility.Hidden;
            GuildInformationVisibility = Visibility.Hidden;
            AllianceInformationVisibility = Visibility.Hidden;
            CurrentMapInformationVisibility = Visibility.Hidden;

            IsTrackingResetByMapChangeActive = SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive;

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

        #endregion Item list (Normal Mode)

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
            _mainWindow.Dispatcher?.Invoke(() =>
            {
                #region Set MainWindow height and width and center window

                _mainWindow.Height = SettingsController.CurrentSettings.MainWindowHeight;
                _mainWindow.Width = SettingsController.CurrentSettings.MainWindowWidth;
                if (SettingsController.CurrentSettings.MainWindowMaximized)
                {
                    _mainWindow.WindowState = WindowState.Maximized;
                }

                Utilities.CenterWindowOnScreen(_mainWindow);

                #endregion Set MainWindow height and width and center window
            });
        }

        private async Task InitMainWindowDataAsync()
        {
            Translation = new MainWindowTranslation();
            ToolTaskController.SetToolTaskController(this);
            SetUiElements();

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
            IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
            IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;
            IsTrackingMobLoot = SettingsController.CurrentSettings.IsTrackingMobLoot;

            TrackingDungeonsCollectionView = CollectionViewSource.GetDefaultView(TrackingDungeons) as ListCollectionView;
            if (TrackingDungeonsCollectionView != null)
            {
                TrackingDungeonsCollectionView.IsLiveSorting = true;
                TrackingDungeonsCollectionView.CustomSort = new DungeonTrackingNumberComparer();
            }

            TrackingNotificationsCollectionView = CollectionViewSource.GetDefaultView(TrackingNotifications) as ListCollectionView;
            if (TrackingNotificationsCollectionView != null)
            {
                TrackingNotificationsCollectionView.IsLiveSorting = true;
                TrackingNotificationsCollectionView.IsLiveFiltering = true;
                TrackingNotificationsCollectionView.SortDescriptions.Add(new SortDescription(nameof(DateTime), ListSortDirection.Descending));
            }

            // Logging
            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Fame)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFame,
                Name = MainWindowTranslation.Fame
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Silver)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSilver,
                Name = MainWindowTranslation.Silver
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Faction)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFaction,
                Name = MainWindowTranslation.Faction
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.SeasonPoints)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints,
                Name = MainWindowTranslation.SeasonPoints
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.ConsumableLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot,
                Name = MainWindowTranslation.ConsumableLoot
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.EquipmentLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot,
                Name = MainWindowTranslation.EquipmentLoot
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.SimpleLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot,
                Name = MainWindowTranslation.SimpleLoot
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.UnknownLoot)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot,
                Name = MainWindowTranslation.UnknownLoot
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.ShowLootFromMob)
            {
                IsSelected = SettingsController.CurrentSettings.IsLootFromMobShown,
                Name = MainWindowTranslation.ShowLootFromMobs
            });

            LoggingFilters.Add(new LoggingFilterObject(TrackingController, this, LoggingFilterType.Kill)
            {
                IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterKill,
                Name = MainWindowTranslation.ShowKills
            });

            // Mails
            MailCollectionView = CollectionViewSource.GetDefaultView(Mails) as ListCollectionView;
            if (MailCollectionView != null)
            {
                MailCollectionView.IsLiveSorting = true;
                MailCollectionView.IsLiveFiltering = true;
                MailCollectionView.SortDescriptions.Add(new SortDescription("Tick", ListSortDirection.Descending));
            }

            MailCollectionView?.Refresh();
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

        public void DungeonStatsGridToggle()
        {
            var unfoldGridHeight = 290;
            var foldGridHeight = 82;

            if (_isDungeonStatsGridUnfold)
            {
                DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleDown;
                DungeonStatsGridHeight = foldGridHeight;
                DungeonStatsScrollViewerMargin = new Thickness(0, foldGridHeight, 0, 0);
                _isDungeonStatsGridUnfold = false;
            }
            else
            {
                DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleUp;
                DungeonStatsGridHeight = unfoldGridHeight;
                DungeonStatsScrollViewerMargin = new Thickness(0, unfoldGridHeight, 0, 0);
                _isDungeonStatsGridUnfold = true;
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

        #region Tracking Mode

        public void StartTracking()
        {
            if (NetworkManager.IsNetworkCaptureRunning)
            {
                return;
            }

            TrackingController?.RegisterEvents();
            TrackingController?.DungeonController?.LoadDungeonFromFile();
            TrackingController?.DungeonController?.SetDungeonStatsDayUi();
            TrackingController?.DungeonController?.SetDungeonStatsTotalUi();
            TrackingController?.DungeonController?.SetOrUpdateDungeonsDataUiAsync();
            TrackingController?.StatisticController?.LoadFromFile();
            TrackingController?.MailController?.LoadFromFile();

            TrackingController?.CountUpTimer.Start();

            DungeonStatsFilter = new DungeonStatsFilter(TrackingController);

            IsTrackingActive = NetworkManager.StartNetworkCapture(this, TrackingController);
            Console.WriteLine(@"### Start Tracking...");
        }

        public void StopTracking()
        {
            TrackingController?.DungeonController?.SaveDungeonsInFile();
            TrackingController?.StatisticController?.SaveInFile();
            TrackingController?.MailController?.SaveInFile();
            TrackingController?.UnregisterEvents();
            TrackingController?.CountUpTimer?.Stop();

            NetworkManager.StopNetworkCapture();

            IsTrackingActive = false;
            Console.WriteLine(@"### Stop Tracking");
        }
        
        public void ResetMainCounters()
        {
            TrackingController?.CountUpTimer?.Reset();
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
                Application.Current.Dispatcher.Invoke(() => TopLooters.Clear());
                TrackingController.LootController.ClearLootLogger();
            }
        }

        public async Task ResetPartyAsync()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_PARTY"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_THE_PARTY"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                await TrackingController.EntityController.ResetPartyMemberAsync();
            }
        }

        public void DeleteSelectedDungeons()
        {
            var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_DUNGEONS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_DUNGEONS"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                var selectedDungeons = TrackingDungeons.Where(x => x.IsSelectedForDeletion ?? false).Select(x => x.DungeonHash);
                TrackingController.DungeonController.RemoveDungeonByHashAsync(selectedDungeons);
            }
        }

        public void ResetDungeonCounters()
        {
            DungeonStatsTotal.EnteredDungeon = 0;
            DungeonStatsTotal.OpenedStandardChests = 0;
            DungeonStatsTotal.OpenedUncommonChests = 0;
            DungeonStatsTotal.OpenedRareChests = 0;
            DungeonStatsTotal.OpenedLegendaryChests = 0;

            DungeonStatsDay.EnteredDungeon = 0;
            DungeonStatsDay.OpenedStandardChests = 0;
            DungeonStatsDay.OpenedUncommonChests = 0;
            DungeonStatsDay.OpenedRareChests = 0;
            DungeonStatsDay.OpenedLegendaryChests = 0;
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

        public void CopyDamageMeterToClipboard()
        {
            var output = string.Empty;
            var counter = 1;

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Damage)
            {
                Clipboard.SetDataObject(DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Dps)
            {
                Clipboard.SetDataObject(DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Dps:N2} DPS\n"));
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Heal)
            {
                Clipboard.SetDataObject(DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Heal} Heal\n"));
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Hps)
            {
                Clipboard.SetDataObject(DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Hps:N2} HPS\n"));
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Name)
            {
                Clipboard.SetDataObject(DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));
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

        public Visibility IsMailMonitoringPopupVisible
        {
            get => _isMailMonitoringPopupVisible;
            set
            {
                _isMailMonitoringPopupVisible = value;
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

        public bool IsTrackingSilver
        {
            get => _isTrackingSilver;
            set
            {
                _isTrackingSilver = value;

                SettingsController.CurrentSettings.IsTrackingSilver = _isTrackingSilver;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFame
        {
            get => _isTrackingFame;
            set
            {
                _isTrackingFame = value;

                SettingsController.CurrentSettings.IsTrackingFame = _isTrackingFame;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingMobLoot
        {
            get => _isTrackingMobLoot;
            set
            {
                _isTrackingMobLoot = value;

                SettingsController.CurrentSettings.IsTrackingMobLoot = _isTrackingMobLoot;
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

        public ObservableCollection<ClusterInfo> EnteredCluster
        {
            get => _enteredCluster;
            set
            {
                _enteredCluster = value;
                OnPropertyChanged();
            }
        }

        public Visibility UsernameInformationVisibility
        {
            get => _usernameInformationVisibility;
            set
            {
                _usernameInformationVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility GuildInformationVisibility
        {
            get => _guildInformationVisibility;
            set
            {
                _guildInformationVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility AllianceInformationVisibility
        {
            get => _allianceInformationVisibility;
            set
            {
                _allianceInformationVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CurrentMapInformationVisibility
        {
            get => _currentMapInformationVisibility;
            set
            {
                _currentMapInformationVisibility = value;
                OnPropertyChanged();
            }
        }

        public DungeonStats DungeonStatsDay
        {
            get => _dungeonStatsDay;
            set
            {
                _dungeonStatsDay = value;
                OnPropertyChanged();
            }
        }

        public DungeonStats DungeonStatsTotal
        {
            get => _dungeonStatsTotal;
            set
            {
                _dungeonStatsTotal = value;
                OnPropertyChanged();
            }
        }

        public DungeonStatsFilter DungeonStatsFilter
        {
            get => _dungeonStatsFilter;
            set
            {
                _dungeonStatsFilter = value;
                OnPropertyChanged();
            }
        }

        public string TrackingUsername
        {
            get => _trackingUsername;
            set
            {
                _trackingUsername = value;
                UsernameInformationVisibility = !string.IsNullOrEmpty(_trackingUsername) ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public string TrackingGuildName
        {
            get => _trackingGuildName;
            set
            {
                _trackingGuildName = value;
                GuildInformationVisibility = !string.IsNullOrEmpty(_trackingGuildName) ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public string TrackingAllianceName
        {
            get => _trackingAllianceName;
            set
            {
                _trackingAllianceName = value;
                AllianceInformationVisibility = !string.IsNullOrEmpty(_trackingAllianceName) ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public string TrackingCurrentMapName
        {
            get => _trackingCurrentMapName;
            set
            {
                _trackingCurrentMapName = value;
                CurrentMapInformationVisibility = !string.IsNullOrEmpty(_trackingCurrentMapName) ? Visibility.Visible : Visibility.Hidden;
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

        public DungeonCloseTimer DungeonCloseTimer
        {
            get => _dungeonCloseTimer;
            set
            {
                _dungeonCloseTimer = value;
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

        public ObservableCollection<TrackingNotification> TrackingNotifications
        {
            get => _trackingNotifications;
            set
            {
                _trackingNotifications = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DungeonNotificationFragment> TrackingDungeons
        {
            get => _trackingDungeons;
            set
            {
                _trackingDungeons = value;
                OnPropertyChanged();
            }
        }

        public ListCollectionView TrackingDungeonsCollectionView
        {
            get => _trackingDungeonsCollectionView;
            set
            {
                _trackingDungeonsCollectionView = value;
                OnPropertyChanged();
            }
        }

        public ListCollectionView TrackingNotificationsCollectionView
        {
            get => _trackingNotificationsCollectionView;
            set
            {
                _trackingNotificationsCollectionView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TopLooterObject> TopLooters
        {
            get => _topLooters;
            set
            {
                _topLooters = value;
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
                        TrackingActiveText = MainWindowTranslation.TrackingIsPartiallyActive;
                        TrackingActivityColor = TrackingIconType.Partially;
                        break;
                    case true when TrackingController is { ExistIndispensableInfos: true }:
                        TrackingActiveText = MainWindowTranslation.TrackingIsActive;
                        TrackingActivityColor = TrackingIconType.On;
                        break;
                    case false:
                        TrackingActiveText = MainWindowTranslation.TrackingIsNotActive;
                        TrackingActivityColor = TrackingIconType.Off;
                        break;
                }

                OnPropertyChanged();
            }
        }

        public TrackingIconType TrackingActivityColor
        {
            get => _trackingActivityColor;
            set
            {
                _trackingActivityColor = value;
                OnPropertyChanged();
            }
        }

        public string TrackingActiveText
        {
            get => _trackingActiveText;
            set
            {
                _trackingActiveText = value;
                OnPropertyChanged();
            }
        }

        public Visibility CharacterIsNotTrackedInfoVisibility
        {
            get => _characterIsNotTrackedInfoVisibility;
            set
            {
                _characterIsNotTrackedInfoVisibility = value;
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

        public double DungeonStatsGridHeight
        {
            get => _dungeonStatsGridHeight;
            set
            {
                _dungeonStatsGridHeight = value;
                OnPropertyChanged();
            }
        }

        public Thickness DungeonStatsScrollViewerMargin
        {
            get => _dungeonStatsScrollViewerMargin;
            set
            {
                _dungeonStatsScrollViewerMargin = value;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon DungeonStatsGridButtonIcon
        {
            get => _dungeonStatsGridButtonIcon;
            set
            {
                _dungeonStatsGridButtonIcon = value;
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

        public ObservableCollection<LoggingFilterObject> LoggingFilters
        {
            get => _loggingFilters;
            set
            {
                _loggingFilters = value;
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

        public ObservableCollection<TaskTextObject> ToolTaskObjects
        {
            get => _toolTaskObjects;
            set
            {
                _toolTaskObjects = value;
                OnPropertyChanged();
            }
        }

        public ListCollectionView MailCollectionView
        {
            get => _mailCollectionView;
            set
            {
                _mailCollectionView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollectionEx<Mail> Mails
        {
            get => _mails;
            set
            {
                _mails = value;
                OnPropertyChanged();
            }
        }
        
        public MailStatsObject MailStatsObject
        {
            get => _mailStatsObject;
            set
            {
                _mailStatsObject = value;
                OnPropertyChanged();
            }
        }

        public string MailsSearchText
        {
            get => _mailsSearchText;
            set
            {
                _mailsSearchText = value;
                MailCollectionView.Filter = TrackingController.MailController.Filter;
                MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
                OnPropertyChanged();
            }
        }

        public DateTime DatePickerMailsFrom
        {
            get => _datePickerMailsFrom;
            set
            {
                _datePickerMailsFrom = value;
                MailCollectionView.Filter = TrackingController.MailController.Filter;
                MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
                OnPropertyChanged();
            }
        }

        public DateTime DatePickerMailsTo
        {
            get => _datePickerMailsTo;
            set
            {
                _datePickerMailsTo = value;
                MailCollectionView.Filter = TrackingController.MailController.Filter;
                MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
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

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Bindings

        #region Structs

        public struct ModeStruct
        {
            public string Name { get; set; }
            public ViewMode ViewMode { get; set; }
        }

        public struct DamageMeterSortStruct
        {
            public string Name { get; set; }
            public DamageMeterSortType DamageMeterSortType { get; set; }
        }

        #endregion
    }

    public enum ViewMode
    {
        Normal,
        Tracking,
        Player,
        Gold
    }
}