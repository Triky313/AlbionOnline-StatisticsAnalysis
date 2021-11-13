using FontAwesome5;
using log4net;
using Microsoft.Win32;
using StatisticsAnalysisTool.Annotations;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
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
        private readonly Dictionary<ViewMode, Grid> viewModeGrid = new();
        private Visibility _allianceInformationVisibility;
        private double _allianceInfoWidth;
        private int _currentGoldPrice;
        private string _currentGoldPriceTimestamp;
        private Visibility _currentMapInformationVisibility;
        private double _currentMapInfoWidth;
        private ObservableCollection<DamageMeterFragment> _damageMeter = new();
        private List<DamageMeterSortStruct> _damageMeterSort = new();
        private DamageMeterSortStruct _damageMeterSortSelection;
        private DungeonStats _dungeonStatsDay = new();
        private DungeonStats _dungeonStatsTotal = new();
        private string _errorBarText;
        private Visibility _errorBarVisibility;
        private Visibility _goldPriceVisibility;
        private Visibility _guildInformationVisibility;
        private double _guildInfoWidth;
        private Visibility _isDamageMeterPopupVisible = Visibility.Hidden;
        private bool _isDamageMeterResetByMapChangeActive;
        private bool _isFullItemInfoLoading;
        private bool _isFullItemInfoSearchActive;
        private bool _isLoadFullItemInfoButtonEnabled;
        private bool _isShowOnlyItemsWithAlertOnActive;
        private bool _isTrackingActive;
        private bool _isTrackingResetByMapChangeActive;
        private bool _isTxtSearchEnabled;
        private Dictionary<Category, string> _itemCategories = new();
        private Visibility _itemCategoriesVisibility;
        private string _itemCounterString;
        private Dictionary<ItemLevel, string> _itemLevels = new();
        private Visibility _itemLevelsVisibility;
        private Dictionary<ParentCategory, string> _itemParentCategories = new();
        private Visibility _itemParentCategoriesVisibility;
        private ICollectionView _itemsView;
        private Dictionary<ItemTier, string> _itemTiers = new();
        private Visibility _itemTiersVisibility;
        //private List<Axis> _xAxes;
        //private List<Axis> _yAxes;
        //private ObservableCollection<ISeries> _series;
        private Visibility _loadFullItemInfoButtonVisibility;
        private string _loadFullItemInfoProBarCounter;
        private Visibility _loadFullItemInfoProBarGridVisibility;
        private int _loadFullItemInfoProBarMax;
        private int _loadFullItemInfoProBarMin;
        private int _loadFullItemInfoProBarValue;
        private Visibility _loadIconVisibility;
        private string _loadTranslation;
        private int _localImageCounter;
        private ObservableCollection<ModeStruct> _modes = new();
        private ModeStruct _modeSelection;
        private string _numberOfValuesTranslation;
        private ObservableCollection<PartyMemberCircle> _partyMemberCircles = new();
        private PlayerModeTranslation _playerModeTranslation = new();
        private string _savedPlayerInformationName;
        private string _searchText;
        private Category _selectedItemCategories;
        private ItemLevel _selectedItemLevel;
        private ParentCategory _selectedItemParentCategories;
        private ItemTier _selectedItemTier;
        private string _famePerHour = "0";
        private string _reSpecPointsPerHour = "0";
        private string _silverPerHour = "0";
        private string _textBoxGoldModeNumberOfValues;
        private string _totalGainedFameInSessionInSession = "0";
        private string _totalGainedReSpecPointsInSessionInSession = "0";
        private string _totalGainedSilverInSessionInSession = "0";
        private Brush _trackerActivationToggleColor;
        private EFontAwesomeIcon _trackerActivationToggleIcon = EFontAwesomeIcon.Solid_ToggleOff;
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
        private DateTime? activateWaitTimer;
        public AlertController AlertManager;
        private ObservableCollection<MainStatObject> _factionPointStats = new() { new MainStatObject() { Value = "0", ValuePerHour = "0", CityFaction = CityFaction.Unknown } };
        private string _mainTrackerTimer;
        private Visibility _isMainTrackerPopupVisible = Visibility.Hidden;
        private bool _isShowOnlyFavoritesActive;
        private DungeonCloseTimer _dungeonCloseTimer;
        private EFontAwesomeIcon _dungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleDown;
        private double _dungeonStatsGridHeight = 82;
        private Thickness _dungeonStatsScrollViewerMargin = new(0, 82, 0, 0);
        private bool IsDungeonStatsGridUnfold;
        private DungeonStatsFilter _dungeonStatsFilter;
        private TrackingIconType _trackingIconColor;
        private bool _isTrackingFilteredEquipmentLoot = true;
        private bool _isTrackingFilteredConsumableLoot = true;
        private bool _isTrackingFilteredSimpleLoot = true;
        private bool _isTrackingFilteredUnknownLoot = true;
        private bool _isTrackingFilteredFame = true;
        private bool _isTrackingFilteredSilver = true;
        private bool _isTrackingFilteredFaction = true;
        private int _partyMemberNumber;
        private ObservableCollection<ClusterInfo> _enteredCluster = new();
        private bool _isItemSearchCheckboxesEnabled;
        private bool _isFilterResetEnabled;
        private Visibility _gridTryToLoadTheItemListAgainVisibility;
        private EFontAwesomeIcon _damageMeterActivationToggleIcon = EFontAwesomeIcon.Solid_ToggleOff;
        private Brush _damageMeterActivationToggleColor;
        private bool _isDamageMeterTrackingActive;
        private ListCollectionView _trackingDungeonsCollectionView;
        private bool _isTrackingFilteredSeasonPoints;
        private ListCollectionView _trackingNotificationsCollectionView;
        private bool _isTrackingPartyLootOnly;
        private bool _isTrackingSilver;
        private bool _isTrackingFame;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            SettingsController.Load();
            _ = CraftingController.LoadAsync();
            InitViewModeGrids();
            UpgradeSettings();
            InitWindowSettings();
            Utilities.AutoUpdate();

            if (!LanguageController.InitializeLanguage())
            {
                _mainWindow.Close();
            }

            InitMainWindowData();
            _ = InitTrackingAsync();
        }

        public async void SetUiElements()
        {
            #region Error bar

            ErrorBarVisibility = Visibility.Hidden;

            #endregion

            #region Set Modes to combobox

            Modes.Clear();
            Modes.Add(new ModeStruct {Name = LanguageController.Translation("NORMAL"), ViewMode = ViewMode.Normal});
            Modes.Add(new ModeStruct {Name = LanguageController.Translation("TRACKING"), ViewMode = ViewMode.Tracking});
            Modes.Add(new ModeStruct {Name = LanguageController.Translation("PLAYER"), ViewMode = ViewMode.Player});
            //Modes.Add(new ModeStruct {Name = LanguageController.Translation("GOLD"), ViewMode = ViewMode.Gold});
            ModeSelection = Modes.FirstOrDefault(x => x.ViewMode == ViewMode.Normal);
            
            #endregion Set Modes to combobox

            #region Full Item Info elements

            LoadFullItemInfoButtonVisibility = Visibility.Hidden;

            IsFullItemInfoSearchActive = Settings.Default.IsFullItemInfoSearchActive;

            ItemParentCategories = CategoryController.ParentCategoryNames;
            SelectedItemParentCategory = ParentCategory.Unknown;

            ItemTiers = FrequentlyValues.ItemTiers;
            SelectedItemTier = ItemTier.Unknown;

            ItemLevels = FrequentlyValues.ItemLevels;
            SelectedItemLevel = ItemLevel.Unknown;

            if (!IsFullItemInfoLoading) LoadFullItemInfoProBarGridVisibility = Visibility.Hidden;

            #endregion Full Item Info elements

            #region Gold price

            var currentGoldPrice = await ApiController.GetGoldPricesFromJsonAsync(null, 1).ConfigureAwait(true);
            CurrentGoldPrice = currentGoldPrice.FirstOrDefault()?.Price ?? 0;
            if (currentGoldPrice.Count > 0)
            {
                CurrentGoldPriceTimestamp = currentGoldPrice.FirstOrDefault()?.Timestamp.ToString(CultureInfo.CurrentCulture) ??
                                            new DateTime(0, 0, 0, 0, 0, 0).ToString(CultureInfo.CurrentCulture);
                GoldPriceVisibility = Visibility.Visible;
            }
            else
            {
                GoldPriceVisibility = Visibility.Hidden;
            }

            #endregion Gold price

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
                var imageAwesome = (ImageAwesome) sender;
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

        #region Item list (Normal Mode)

        public void ItemFilterReset()
        {
            SearchText = string.Empty;
            SelectedItemCategory = Category.Unknown;
            SelectedItemParentCategory = ParentCategory.Unknown;
            SelectedItemLevel = ItemLevel.Unknown;
            SelectedItemTier = ItemTier.Unknown;
        }

        #endregion Item list (Normal Mode)

        #region Helper methods

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
                Log.Fatal(nameof(OnUnhandledException), (Exception) e.ExceptionObject);
            }
            catch (Exception ex)
            {
                Log.Fatal(nameof(OnUnhandledException), ex);
            }
        }
        
        #region View mode init

        private void InitViewModeGrids()
        {
            viewModeGrid.Add(ViewMode.Normal, _mainWindow.GridNormalMode);
            viewModeGrid.Add(ViewMode.Tracking, _mainWindow.GridTrackingMode);
            viewModeGrid.Add(ViewMode.Player, _mainWindow.GridPlayerMode);
            //viewModeGrid.Add(ViewMode.Gold, _mainWindow.GridGoldMode);
        }

        public void SelectViewModeGrid()
        {
            HideAllGrids();
            var grid = viewModeGrid.FirstOrDefault(g => g.Key == ModeSelection.ViewMode);

            if (grid.Value == null)
            {
                Log.Warn($"SelectViewModeGrid: Grid for [{ModeSelection.ViewMode}] is not existing");

                if (viewModeGrid.FirstOrDefault().Value != null) viewModeGrid.FirstOrDefault().Value.Visibility = Visibility.Visible;

                return;
            }

            grid.Value.Visibility = Visibility.Visible;
            _mainWindow.TxtSearch.Focus();
        }

        private void HideAllGrids()
        {
            if (viewModeGrid == null) return;

            foreach (var grid in viewModeGrid) grid.Value.Visibility = Visibility.Hidden;
        }

        #endregion

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

        private void InitWindowSettings()
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

                CenterWindowOnScreen();

                #endregion Set MainWindow height and width and center window
            });
        }

        private async void InitMainWindowData()
        {
            Translation = new MainWindowTranslation();

            SetUiElements();

            ShowInfoWindow();
            TextBoxGoldModeNumberOfValues = "10";

            await InitItemListAsync().ConfigureAwait(false);
        }

        public async Task InitItemListAsync()
        {
            IsTxtSearchEnabled = false;
            IsItemSearchCheckboxesEnabled = false;
            IsFilterResetEnabled = false;
            LoadIconVisibility = Visibility.Visible;
            GridTryToLoadTheItemListAgainVisibility = Visibility.Collapsed;

            var isItemListLoaded = await ItemController.GetItemListFromJsonAsync().ConfigureAwait(true);
            if (!isItemListLoaded)
            {
                SetErrorBar(Visibility.Visible, LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"));
                GridTryToLoadTheItemListAgainVisibility = Visibility.Visible;

                return;
            }

            await ItemController.SetFavoriteItemsFromLocalFileAsync();
            await ItemController.GetItemInformationListFromLocalAsync();
            IsFullItemInformationCompleteCheck();

            ItemsView = new ListCollectionView(ItemController.Items);
            InitAlerts();

            LoadIconVisibility = Visibility.Hidden;
            IsFilterResetEnabled = true;
            IsItemSearchCheckboxesEnabled = true;
            IsTxtSearchEnabled = true;

            _mainWindow.Dispatcher?.Invoke(() => { _ = _mainWindow.TxtSearch.Focus(); });
        }

        private async Task InitTrackingAsync()
        {
            _ = await WorldData.GetDataListFromJsonAsync();
            _ = await DungeonObjectData.GetDataListFromJsonAsync();

            TrackingController ??= new TrackingController(this, _mainWindow);

            if (SettingsController.CurrentSettings.IsTrackingActiveAtToolStart)
            {
                _ = StartTrackingAsync();
            }

            IsTrackingFilteredSilver = SettingsController.CurrentSettings.IsMainTrackerFilterSilver;
            IsTrackingFilteredFame = SettingsController.CurrentSettings.IsMainTrackerFilterFame;
            IsTrackingFilteredFaction = SettingsController.CurrentSettings.IsMainTrackerFilterFaction;
            IsTrackingFilteredEquipmentLoot = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot;
            IsTrackingFilteredConsumableLoot = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot;
            IsTrackingFilteredSimpleLoot = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot;
            IsTrackingFilteredUnknownLoot = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot;
            IsTrackingFilteredSeasonPoints = SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints;
            IsDamageMeterTrackingActive = SettingsController.CurrentSettings.IsDamageMeterTrackingActive;
            IsTrackingPartyLootOnly = SettingsController.CurrentSettings.IsTrackingPartyLootOnly;
            IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
            IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;

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
                TrackingNotificationsCollectionView.SortDescriptions.Add(new SortDescription(nameof(DateTime), ListSortDirection.Descending));
            }
        }

        #endregion

        #region Save Settings

        public void SaveSettings(WindowState windowState, Rect restoreBounds, double height, double width)
        {
            #region Window

            if (windowState == WindowState.Maximized)
            {
                SettingsController.CurrentSettings.MainWindowHeight = restoreBounds.Height;
                SettingsController.CurrentSettings.MainWindowWidth = restoreBounds.Width;
                SettingsController.CurrentSettings.MainWindowMaximized = true;
            }
            else
            {
                SettingsController.CurrentSettings.MainWindowHeight = height;
                SettingsController.CurrentSettings.MainWindowWidth = width;
                SettingsController.CurrentSettings.MainWindowMaximized = false;
            }

            #endregion

            SettingsController.Save();

            ItemController.SaveFavoriteItemsToLocalFile();
            ItemController.SaveItemInformationLocal();
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

        public void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = _mainWindow.Width;
            var windowHeight = _mainWindow.Height;
            _mainWindow.Left = screenWidth / 2 - windowWidth / 2;
            _mainWindow.Top = screenHeight / 2 - windowHeight / 2;
        }

        private void ShowInfoWindow()
        {
            if (SettingsController.CurrentSettings.IsInfoWindowShownOnStart)
            {
                var infoWindow = new InfoWindow();
                infoWindow.Show();
            }
        }

        public void OpenDamageMeterWindow()
        {
            try
            {
                if (Utilities.IsWindowOpen<DamageMeterWindow>())
                {
                    var existItemWindow = Application.Current.Windows.OfType<DamageMeterWindow>().FirstOrDefault();
                    existItemWindow?.Activate();
                }
                else
                {
                    var itemWindow = new DamageMeterWindow(DamageMeter);
                    itemWindow.Show();
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

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

            if (IsDungeonStatsGridUnfold)
            {
                DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleDown;
                DungeonStatsGridHeight = foldGridHeight;
                DungeonStatsScrollViewerMargin = new Thickness(0, foldGridHeight, 0, 0);
                IsDungeonStatsGridUnfold = false;
            }
            else
            {
                DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleUp;
                DungeonStatsGridHeight = unfoldGridHeight;
                DungeonStatsScrollViewerMargin = new Thickness(0, unfoldGridHeight, 0,0);
                IsDungeonStatsGridUnfold = true;
            }
        }

        public void ExportLootToFile()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"log-{DateTime.UtcNow:yy-MMM-dd}",
                DefaultExt = ".csv",
                Filter = "CSV documents (.csv)|*.csv"
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, TrackingController.LootController.GetLootLoggerObjectsAsCsv());
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }
        
        #endregion

        #region Full Item Information

        public void IsFullItemInformationCompleteCheck()
        {
            if (ItemController.IsFullItemInformationComplete)
            {
                LoadFullItemInfoButtonVisibility = Visibility.Hidden;
                IsLoadFullItemInfoButtonEnabled = false;
                LoadFullItemInfoProBarGridVisibility = Visibility.Hidden;
            }
            else
            {
                LoadFullItemInfoButtonVisibility = Visibility.Visible;
                IsLoadFullItemInfoButtonEnabled = true;
            }
        }

        public async void LoadAllFullItemInformationFromWeb()
        {
            IsLoadFullItemInfoButtonEnabled = false;
            LoadFullItemInfoButtonVisibility = Visibility.Hidden;
            LoadFullItemInfoProBarGridVisibility = Visibility.Visible;

            LoadFullItemInfoProBarMin = 0;
            LoadFullItemInfoProBarValue = 0;
            LoadFullItemInfoProBarMax = ItemController.Items.Count;
            IsFullItemInfoLoading = true;

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5,
                BoundedCapacity = 50
            };

            var block = new ActionBlock<Item>(async item =>
            {
                item.FullItemInformation = await ItemController.GetFullItemInformationAsync(item);
            }, options);

            await foreach (var item in ItemController.Items.ToAsyncEnumerable())
            {
                if (!IsFullItemInfoLoading)
                {
                    break;
                }

                if (await block.SendAsync(item))
                {
                    LoadFullItemInfoProBarValue++;
                }
            }

            block.Complete();
            await block.Completion;

            LoadFullItemInfoProBarGridVisibility = Visibility.Hidden;

            if (ItemController.IsFullItemInformationComplete)
            {
                LoadFullItemInfoButtonVisibility = Visibility.Hidden;
                IsLoadFullItemInfoButtonEnabled = false;
            }
            else
            {
                LoadFullItemInfoButtonVisibility = Visibility.Visible;
                IsLoadFullItemInfoButtonEnabled = true;
            }
        }

        #endregion

        #region Player information (Player Mode)

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

        #region Gold (Gold Mode)

#pragma warning disable 1998
        public async void SetGoldChart(int count)
#pragma warning restore 1998
        {
            //var goldPriceList = await ApiController.GetGoldPricesFromJsonAsync(null, count).ConfigureAwait(true) as IEnumerable<GoldResponseModel>;
            //var values = goldPriceList.Select(x => x.Price);

            //Series = new ObservableCollection<ISeries>
            //{
            //    new ColumnSeries<int>
            //    {
            //        Values = values
            //    }
            //};

            //XAxes = new List<Axis>
            //{
            //    new()
            //    {
            //        IsVisible = true,
            //        MaxLimit = goldPriceList.Count(),
            //        //Labels = goldPriceList.Select(x => x.Timestamp.ToString(CultureInfo.CurrentCulture)) as IList<string>,
            //        ShowSeparatorLines = false
            //    }
            //};

            //YAxes = new List<Axis>
            //{
            //    new()
            //    {
            //        IsVisible = true,
            //        MaxLimit = goldPriceList.Max(x => x.Price + 1),
            //        MinLimit = goldPriceList.Min(x => x.Price - 1),
            //        ShowSeparatorLines = false
            //    }
            //};
        }

        //public ObservableCollection<ISeries> Series
        //{
        //    get => _series;
        //    set
        //    {
        //        _series = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public List<Axis> XAxes 
        //{
        //    get => _xAxes;
        //    set
        //    {
        //        _xAxes = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public List<Axis> YAxes 
        //{
        //    get => _yAxes;
        //    set
        //    {
        //        _yAxes = value;
        //        OnPropertyChanged();
        //    }
        //}

        #endregion Gold (Gold Mode)

        #region Tracking Mode

        public void TrackerActivationToggle()
        {
            if (!IsReadyToTracking())
            {
                return;
            }

            IsTrackingActive = !IsTrackingActive;

            if (IsTrackingActive)
            {
                _ = StartTrackingAsync();
            }
            else
            {
                StopTracking();
            }
        }

        public async Task StartTrackingAsync()
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

            TrackingController?.CountUpTimer.Start();

            DungeonStatsFilter = new DungeonStatsFilter(TrackingController);

            IsTrackingActive = await NetworkManager.StartNetworkCaptureAsync(this, TrackingController);
            Console.WriteLine(@"### Start Tracking...");
        }

        public void StopTracking()
        {
            TrackingController?.DungeonController?.SaveDungeonsInFile();
            TrackingController?.UnregisterEvents();
            TrackingController?.CountUpTimer?.Stop();

            NetworkManager.StopNetworkCapture();

            IsTrackingActive = false;
            Console.WriteLine(@"### Stop Tracking");
        }

        private bool IsReadyToTracking()
        {
            var waitTime = activateWaitTimer?.AddSeconds(1);
            if (waitTime < DateTime.Now || waitTime == null)
            {
                activateWaitTimer = DateTime.Now;
                return true;
            }

            return false;
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

        private void SetIsDamageMeterShowing(IEnumerable<DamageMeterFragment> damageMeter, bool isDamageMeterShowing)
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

            if (IsFullItemInfoSearchActive)
                ItemsView.Filter = i =>
                {
                    var item = i as Item;
                    if (IsShowOnlyItemsWithAlertOnActive)
                    {
                        return item?.FullItemInformation != null &&
                               item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                               && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                                   SelectedItemParentCategory == ParentCategory.Unknown)
                               && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                               && ((ItemTier) item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                               && ((ItemLevel) item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                               && item.IsAlertActive;
                    }

                    if (IsShowOnlyFavoritesActive)
                    {
                        return item?.FullItemInformation != null &&
                               item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                               && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                                   SelectedItemParentCategory == ParentCategory.Unknown)
                               && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                               && ((ItemTier)item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                               && ((ItemLevel)item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)
                               && item.IsFavorite;
                    }

                    return item?.FullItemInformation != null &&
                           item.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                           && (item.FullItemInformation?.CategoryObject?.ParentCategory == SelectedItemParentCategory ||
                               SelectedItemParentCategory == ParentCategory.Unknown)
                           && (item.FullItemInformation?.CategoryObject?.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                           && ((ItemTier) item.FullItemInformation?.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                           && ((ItemLevel) item.FullItemInformation?.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown);
                };
            else
                ItemsView.Filter = i =>
                {
                    var item = i as Item;

                    if (IsShowOnlyItemsWithAlertOnActive)
                    {
                        return (item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false) && item.IsAlertActive;
                    }

                    if (IsShowOnlyFavoritesActive)
                    {
                        return (item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false) && item.IsFavorite;
                    }

                    return item?.LocalizedNameAndEnglish.ToLower().Contains(SearchText?.ToLower() ?? string.Empty) ?? false;
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

        public Visibility IsMainTrackerPopupVisible
        {
            get => _isMainTrackerPopupVisible;
            set
            {
                _isMainTrackerPopupVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredEquipmentLoot
        {
            get => _isTrackingFilteredEquipmentLoot;
            set
            {
                _isTrackingFilteredEquipmentLoot = value;

                if (_isTrackingFilteredEquipmentLoot)
                {
                    TrackingController?.AddFilterType(NotificationType.EquipmentLoot);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.EquipmentLoot);
                }

                TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot = _isTrackingFilteredEquipmentLoot;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredConsumableLoot
        {
            get => _isTrackingFilteredConsumableLoot;
            set
            {
                _isTrackingFilteredConsumableLoot = value;

                if (_isTrackingFilteredConsumableLoot)
                {
                    TrackingController?.AddFilterType(NotificationType.ConsumableLoot);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.ConsumableLoot);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot = _isTrackingFilteredConsumableLoot;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredSimpleLoot
        {
            get => _isTrackingFilteredSimpleLoot;
            set
            {
                _isTrackingFilteredSimpleLoot = value;

                if (_isTrackingFilteredSimpleLoot)
                {
                    TrackingController?.AddFilterType(NotificationType.SimpleLoot);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.SimpleLoot);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot = _isTrackingFilteredSimpleLoot;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredUnknownLoot
        {
            get => _isTrackingFilteredUnknownLoot;
            set
            {
                _isTrackingFilteredUnknownLoot = value;

                if (_isTrackingFilteredUnknownLoot)
                {
                    TrackingController?.AddFilterType(NotificationType.UnknownLoot);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.UnknownLoot);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot = _isTrackingFilteredUnknownLoot;
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

        public bool IsTrackingFilteredFame {
            get => _isTrackingFilteredFame;
            set
            {
                _isTrackingFilteredFame = value;

                if (_isTrackingFilteredFame)
                {
                    TrackingController?.AddFilterType(NotificationType.Fame);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.Fame);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterFame = _isTrackingFilteredFame;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredSilver {
            get => _isTrackingFilteredSilver;
            set
            {
                _isTrackingFilteredSilver = value;

                if (_isTrackingFilteredSilver)
                {
                    TrackingController?.AddFilterType(NotificationType.Silver);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.Silver);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterSilver = _isTrackingFilteredSilver;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingFilteredFaction {
            get => _isTrackingFilteredFaction;
            set
            {
                _isTrackingFilteredFaction = value;

                if (_isTrackingFilteredFaction)
                {
                    TrackingController?.AddFilterType(NotificationType.Faction);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.Faction);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterFaction = _isTrackingFilteredFaction;
                OnPropertyChanged();
            }
        }
        
        public bool IsTrackingFilteredSeasonPoints {
            get => _isTrackingFilteredSeasonPoints;
            set
            {
                _isTrackingFilteredSeasonPoints = value;

                if (_isTrackingFilteredSeasonPoints)
                {
                    TrackingController?.AddFilterType(NotificationType.SeasonPoints);
                }
                else
                {
                    TrackingController?.RemoveFilterType(NotificationType.SeasonPoints);
                }

                _ = TrackingController?.NotificationUiFilteringAsync();
                SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints = _isTrackingFilteredSeasonPoints;
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

        public Visibility GoldPriceVisibility
        {
            get => _goldPriceVisibility;
            set
            {
                _goldPriceVisibility = value;
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
                UsernameInfoWidth = string.IsNullOrEmpty(_trackingUsername) ? 0 : double.NaN;
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
                GuildInfoWidth = string.IsNullOrEmpty(_trackingGuildName) ? 0 : double.NaN;
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
                AllianceInfoWidth = string.IsNullOrEmpty(_trackingAllianceName) ? 0 : double.NaN;
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
                CurrentMapInfoWidth = string.IsNullOrEmpty(_trackingCurrentMapName) ? 0 : double.NaN;
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

        public string FamePerHour
        {
            get => _famePerHour;
            set
            {
                _famePerHour = value;
                OnPropertyChanged();
            }
        }

        public string SilverPerHour
        {
            get => _silverPerHour;
            set
            {
                _silverPerHour = value;
                OnPropertyChanged();
            }
        }

        public string ReSpecPointsPerHour
        {
            get => _reSpecPointsPerHour;
            set
            {
                _reSpecPointsPerHour = value;
                OnPropertyChanged();
            }
        }

        public string TotalGainedFameInSession
        {
            get => _totalGainedFameInSessionInSession;
            set
            {
                _totalGainedFameInSessionInSession = value;
                OnPropertyChanged();
            }
        }

        public string TotalGainedSilverInSession
        {
            get => _totalGainedSilverInSessionInSession;
            set
            {
                _totalGainedSilverInSessionInSession = value;
                OnPropertyChanged();
            }
        }

        public string TotalGainedReSpecPointsInSession
        {
            get => _totalGainedReSpecPointsInSessionInSession;
            set
            {
                _totalGainedReSpecPointsInSessionInSession = value;
                OnPropertyChanged();
            }
        }

        public string MainTrackerTimer {
            get => _mainTrackerTimer;
            set
            {
                _mainTrackerTimer = value;
                OnPropertyChanged();
            }
        }

        public DungeonCloseTimer DungeonCloseTimer {
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
        
        public ListCollectionView TrackingDungeonsCollectionView {
            get => _trackingDungeonsCollectionView;
            set
            {
                _trackingDungeonsCollectionView = value;
                OnPropertyChanged();
            }
        }

        public ListCollectionView TrackingNotificationsCollectionView {
            get => _trackingNotificationsCollectionView;
            set
            {
                _trackingNotificationsCollectionView = value;
                OnPropertyChanged();
            }
        }

        public bool IsTrackingActive
        {
            get => _isTrackingActive;
            set
            {
                _isTrackingActive = value;

                TrackerActivationToggleIcon = _isTrackingActive ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;

                var colorOn = new SolidColorBrush((Color) Application.Current.Resources["Color.Blue.2"]);
                var colorOff = new SolidColorBrush((Color) Application.Current.Resources["Color.Text.Normal"]);
                TrackerActivationToggleColor = _isTrackingActive ? colorOn : colorOff;

                if (_isTrackingActive && TrackingController is { ExistIndispensableInfos: false })
                {
                    TrackingIconColor = TrackingIconType.Partially;
                }
                else if(_isTrackingActive && TrackingController is { ExistIndispensableInfos: true })
                {
                    TrackingIconColor = TrackingIconType.On;
                }
                else if(!_isTrackingActive)
                {
                    TrackingIconColor = TrackingIconType.Off;
                }

                SettingsController.CurrentSettings.IsTrackingActiveAtToolStart = _isTrackingActive;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon TrackerActivationToggleIcon
        {
            get => _trackerActivationToggleIcon;
            set
            {
                _trackerActivationToggleIcon = value;
                OnPropertyChanged();
            }
        }

        public Brush TrackerActivationToggleColor
        {
            get => _trackerActivationToggleColor ?? new SolidColorBrush((Color) Application.Current.Resources["Color.Text.Normal"]);
            set
            {
                _trackerActivationToggleColor = value;
                OnPropertyChanged();
            }
        }

        public TrackingIconType TrackingIconColor
        {
            get => _trackingIconColor;
            set
            {
                _trackingIconColor = value;
                OnPropertyChanged();
            }
        }

        public bool IsDamageMeterTrackingActive {
            get => _isDamageMeterTrackingActive;
            set {
                if (TrackingController?.CombatController == null)
                {
                    return;
                }

                _isDamageMeterTrackingActive = value;

                TrackingController.CombatController.IsDamageMeterActive = _isDamageMeterTrackingActive;

                DamageMeterActivationToggleIcon = _isDamageMeterTrackingActive ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;

                var colorOn = new SolidColorBrush((Color)Application.Current.Resources["Color.Blue.2"]);
                var colorOff = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
                DamageMeterActivationToggleColor = _isDamageMeterTrackingActive ? colorOn : colorOff;

                SettingsController.CurrentSettings.IsDamageMeterTrackingActive = _isDamageMeterTrackingActive;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon DamageMeterActivationToggleIcon {
            get => _damageMeterActivationToggleIcon;
            set {
                _damageMeterActivationToggleIcon = value;
                OnPropertyChanged();
            }
        }

        public Brush DamageMeterActivationToggleColor {
            get => _damageMeterActivationToggleColor ?? new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
            set {
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

        public bool IsShowOnlyFavoritesActive {
            get => _isShowOnlyFavoritesActive;
            set {
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

        public bool IsFullItemInfoLoading
        {
            get => _isFullItemInfoLoading;
            set
            {
                _isFullItemInfoLoading = value;
                OnPropertyChanged();
            }
        }

        public string LoadFullItemInfoProBarCounter
        {
            get => _loadFullItemInfoProBarCounter;
            set
            {
                _loadFullItemInfoProBarCounter = value;
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

        public Visibility LoadFullItemInfoProBarGridVisibility
        {
            get => _loadFullItemInfoProBarGridVisibility;
            set
            {
                _loadFullItemInfoProBarGridVisibility = value;
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarValue
        {
            get => _loadFullItemInfoProBarValue;
            set
            {
                _loadFullItemInfoProBarValue = value;
                LoadFullItemInfoProBarCounter = $"{_loadFullItemInfoProBarValue}/{LoadFullItemInfoProBarMax}";
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarMax
        {
            get => _loadFullItemInfoProBarMax;
            set
            {
                _loadFullItemInfoProBarMax = value;
                OnPropertyChanged();
            }
        }

        public int LoadFullItemInfoProBarMin
        {
            get => _loadFullItemInfoProBarMin;
            set
            {
                _loadFullItemInfoProBarMin = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoadFullItemInfoButtonEnabled
        {
            get => _isLoadFullItemInfoButtonEnabled;
            set
            {
                _isLoadFullItemInfoButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadFullItemInfoButtonVisibility
        {
            get => _loadFullItemInfoButtonVisibility;
            set
            {
                _loadFullItemInfoButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsFullItemInfoSearchActive
        {
            get => _isFullItemInfoSearchActive;
            set
            {
                _isFullItemInfoSearchActive = value;

                if (_isFullItemInfoSearchActive)
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Visible;
                else
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Hidden;

                ItemsViewFilter();
                ItemsView?.Refresh();

                Settings.Default.IsFullItemInfoSearchActive = _isFullItemInfoSearchActive;
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

        public Visibility ItemLevelsVisibility
        {
            get => _itemLevelsVisibility;
            set
            {
                _itemLevelsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemTiersVisibility
        {
            get => _itemTiersVisibility;
            set
            {
                _itemTiersVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemCategoriesVisibility
        {
            get => _itemCategoriesVisibility;
            set
            {
                _itemCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemParentCategoriesVisibility
        {
            get => _itemParentCategoriesVisibility;
            set
            {
                _itemParentCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<Category, string> ItemCategories
        {
            get => _itemCategories;
            set
            {
                var categories = value;
                categories = new Dictionary<Category, string> {{Category.Unknown, string.Empty}}.Concat(categories)
                    .ToDictionary(k => k.Key, v => v.Value);
                _itemCategories = categories;
                OnPropertyChanged();
            }
        }

        public Category SelectedItemCategory
        {
            get => _selectedItemCategories;
            set
            {
                _selectedItemCategories = value;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }

        public Dictionary<ParentCategory, string> ItemParentCategories
        {
            get => _itemParentCategories;
            set
            {
                _itemParentCategories = value;
                OnPropertyChanged();
            }
        }

        public ParentCategory SelectedItemParentCategory
        {
            get => _selectedItemParentCategories;
            set
            {
                _selectedItemParentCategories = value;
                ItemCategories = CategoryController.GetCategoriesByParentCategory(SelectedItemParentCategory);
                SelectedItemCategory = Category.Unknown;
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

        public ObservableCollection<ModeStruct> Modes
        {
            get => _modes;
            set
            {
                _modes = value;
                OnPropertyChanged();
            }
        }

        public ModeStruct ModeSelection
        {
            get => _modeSelection;
            set
            {
                _modeSelection = value;
                OnPropertyChanged();
            }
        }

        public int CurrentGoldPrice
        {
            get => _currentGoldPrice;
            set
            {
                _currentGoldPrice = value;
                OnPropertyChanged();
            }
        }

        public string CurrentGoldPriceTimestamp
        {
            get => _currentGoldPriceTimestamp;
            set
            {
                _currentGoldPriceTimestamp = value;
                OnPropertyChanged();
            }
        }

        public string TextBoxGoldModeNumberOfValues
        {
            get => _textBoxGoldModeNumberOfValues;
            set
            {
                _textBoxGoldModeNumberOfValues = value;
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

        public double DungeonStatsGridHeight {
            get => _dungeonStatsGridHeight;
            set
            {
                _dungeonStatsGridHeight = value;
                OnPropertyChanged();
            }
        }

        public Thickness DungeonStatsScrollViewerMargin {
            get => _dungeonStatsScrollViewerMargin;
            set
            {
                _dungeonStatsScrollViewerMargin = value;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon DungeonStatsGridButtonIcon {
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

        public string ErrorBarText
        {
            get => _errorBarText;
            set
            {
                _errorBarText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MainStatObject> FactionPointStats {
            get => _factionPointStats;
            set {
                _factionPointStats = value;
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

        public Visibility ErrorBarVisibility
        {
            get => _errorBarVisibility;
            set
            {
                _errorBarVisibility = value;
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

        public static string DonateUrl => Settings.Default.DonateUrl;
        public static string DiscordUrl => Settings.Default.DiscordUrl;
        public static string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;
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