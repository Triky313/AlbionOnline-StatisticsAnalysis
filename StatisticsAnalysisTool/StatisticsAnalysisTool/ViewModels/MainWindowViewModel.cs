using FontAwesome5;
using LiveCharts;
using LiveCharts.Wpf;
using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StatisticsAnalysisTool.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static MainWindow _mainWindow;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static PlayerModeInformationModel _playerModeInformationLocal;
        private static PlayerModeInformationModel _playerModeInformation;
        private readonly Dictionary<ViewMode, Grid> viewModeGrid = new Dictionary<ViewMode, Grid>();
        private Visibility _allianceInformationVisibility;
        private double _allianceInfoWidth;
        private int _currentGoldPrice;
        private string _currentGoldPriceTimestamp;
        private Visibility _currentMapInformationVisibility;
        private double _currentMapInfoWidth;
        private ObservableCollection<DamageMeterFragment> _damageMeter = new ObservableCollection<DamageMeterFragment>();
        private List<DamageMeterSortStruct> _damageMeterSort = new List<DamageMeterSortStruct>();
        private DamageMeterSortStruct _damageMeterSortSelection;
        private DungeonStats _dungeonStatsDay = new DungeonStats();
        private DungeonStats _dungeonStatsTotal = new DungeonStats();
        private string _errorBarText;
        private Visibility _errorBarVisibility;
        private string _fullItemInformationExistLocal;
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
        private Dictionary<Category, string> _itemCategories = new Dictionary<Category, string>();
        private Visibility _itemCategoriesVisibility;
        private string _itemCounterString;
        private Dictionary<ItemLevel, string> _itemLevels = new Dictionary<ItemLevel, string>();
        private Visibility _itemLevelsVisibility;
        private Dictionary<ParentCategory, string> _itemParentCategories = new Dictionary<ParentCategory, string>();
        private Visibility _itemParentCategoriesVisibility;
        private ICollectionView _itemsView;
        private Dictionary<ItemTier, string> _itemTiers = new Dictionary<ItemTier, string>();
        private Visibility _itemTiersVisibility;
        private string[] _labels;
        private Visibility _loadFullItemInfoButtonVisibility;
        private string _loadFullItemInfoProBarCounter;
        private Visibility _loadFullItemInfoProBarGridVisibility;
        private int _loadFullItemInfoProBarMax;
        private int _loadFullItemInfoProBarMin;
        private int _loadFullItemInfoProBarValue;
        private Visibility _loadIconVisibility;
        private string _loadTranslation;
        private int _localImageCounter;
        private ObservableCollection<ModeStruct> _modes = new ObservableCollection<ModeStruct>();
        private ModeStruct _modeSelection;
        private string _numberOfValuesTranslation;
        private ObservableCollection<PartyMemberCircle> _partyMemberCircles = new ObservableCollection<PartyMemberCircle>();
        private PlayerModeTranslation _playerModeTranslation = new PlayerModeTranslation();
        private string _savedPlayerInformationName;
        private string _searchText;
        private Category _selectedItemCategories;
        private ItemLevel _selectedItemLevel;
        private ParentCategory _selectedItemParentCategories;
        private ItemTier _selectedItemTier;
        private SeriesCollection _seriesCollection;
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
        private TrackingController _trackingController;
        private string _trackingCurrentMapName;
        private ObservableCollection<DungeonNotificationFragment> _trackingDungeons = new ObservableCollection<DungeonNotificationFragment>();
        private string _trackingGuildName;
        private ObservableCollection<TrackingNotification> _trackingNotifications = new ObservableCollection<TrackingNotification>();
        private ObservableCollection<TrackingNotification> _debugTrackingNotifications = new ObservableCollection<TrackingNotification>();
        private string _trackingUsername;
        private MainWindowTranslation _translation;
        private string _updateTranslation;
        private Visibility _usernameInformationVisibility;
        private double _usernameInfoWidth;
        private DateTime? activateWaitTimer;
        public AlertController AlertManager;
        private ObservableCollection<MainStatObject> _factionPointStats = new ObservableCollection<MainStatObject>() { new MainStatObject() { Value = "0", ValuePerHour = "0", CityFaction = CityFaction.Unknown } };
        private string _mainTrackerTimer;
        private Visibility _isMainTrackerPopupVisible = Visibility.Hidden;
        private bool _isShowOnlyFavoritesActive;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            InitViewModeGrids();
            UpgradeSettings();
            InitWindowSettings();
            Utilities.AutoUpdate();

            if (!LanguageController.InitializeLanguage())
            {
                _mainWindow.Close();
            }

            InitMainWindowData();
            InitTracking();
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
            Modes.Add(new ModeStruct {Name = LanguageController.Translation("GOLD"), ViewMode = ViewMode.Gold});
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
            if (!currentGoldPrice.IsNullOrEmpty())
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

            SavedPlayerInformationName = Settings.Default.SavedPlayerInformationName;

            #endregion Player information

            #region Tracking

            UsernameInformationVisibility = Visibility.Hidden;
            GuildInformationVisibility = Visibility.Hidden;
            AllianceInformationVisibility = Visibility.Hidden;
            CurrentMapInformationVisibility = Visibility.Hidden;

            IsTrackingResetByMapChangeActive = Settings.Default.IsTrackingResetByMapChangeActive;

            var sortByDamageStruct = new DamageMeterSortStruct
            {
                Name = Translation.SortByDamage,
                DamageMeterSortType = DamageMeterSortType.Damage
            };
            var sortByDpsStruct = new DamageMeterSortStruct
            {
                Name = Translation.SortByDps,
                DamageMeterSortType = DamageMeterSortType.Dps
            };
            var sortByNameStruct = new DamageMeterSortStruct
            {
                Name = Translation.SortByName,
                DamageMeterSortType = DamageMeterSortType.Name
            };

            DamageMeterSort.Clear();
            DamageMeterSort.Add(sortByDamageStruct);
            DamageMeterSort.Add(sortByDpsStruct);
            DamageMeterSort.Add(sortByNameStruct);
            DamageMeterSortSelection = sortByDamageStruct;

            #endregion
        }

        #region Alert

        public void ToggleAlertSender(object sender)
        {
            if (sender == null) return;

            try
            {
                var imageAwesome = (ImageAwesome) sender;
                var item = (Item) imageAwesome.DataContext;

                if (item.AlertModeMinSellPriceIsUndercutPrice <= 0) return;

                item.IsAlertActive = AlertManager.ToggleAlert(ref item);
                ItemsView.Refresh();
            }
            catch (Exception e)
            {
                Log.Error(nameof(ToggleAlertSender), e);
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
            viewModeGrid.Add(ViewMode.Gold, _mainWindow.GridGoldMode);
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

        private void UpgradeSettings()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
        }

        private void InitWindowSettings()
        {
            _mainWindow.Dispatcher?.Invoke(() =>
            {
                #region Set MainWindow height and width and center window

                _mainWindow.Height = Settings.Default.MainWindowHeight;
                _mainWindow.Width = Settings.Default.MainWindowWidth;
                if (Settings.Default.MainWindowMaximized) _mainWindow.WindowState = WindowState.Maximized;

                CenterWindowOnScreen();

                #endregion Set MainWindow height and width and center window
            });
        }

        private async void InitMainWindowData()
        {
            Translation = new MainWindowTranslation();

            SetUiElements();

            IsTxtSearchEnabled = false;
            LoadIconVisibility = Visibility.Visible;

            var isItemListLoaded = await ItemController.GetItemListFromJsonAsync().ConfigureAwait(true);
            if (!isItemListLoaded)
            {
                MessageBox.Show(LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"), LanguageController.Translation("ERROR"));
            }

            if (isItemListLoaded)
            {
                ItemController.SetFavoriteItemsFromLocalFile();

                await ItemController.GetItemInformationListFromLocalAsync();
                IsFullItemInformationCompleteCheck();

                ItemsView = new ListCollectionView(ItemController.Items);
                InitAlerts();

                LoadIconVisibility = Visibility.Hidden;
                IsTxtSearchEnabled = true;

                _mainWindow.Dispatcher?.Invoke(() => { _mainWindow.TxtSearch.Focus(); });
            }

            ShowInfoWindow();
            TextBoxGoldModeNumberOfValues = "10";
        }

        private async void InitTracking()
        {
            await WorldData.GetDataListFromJsonAsync();
            await LootChestData.GetDataListFromJsonAsync();

            if (Settings.Default.IsTrackingActiveAtToolStart)
            {
                StartTracking();
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
            if (Settings.Default.ShowInfoWindowOnStartChecked)
            {
                var infoWindow = new InfoWindow();
                infoWindow.Show();
            }
        }

        public static void OpenItemWindow(Item item)
        {
            if (string.IsNullOrEmpty(item?.UniqueName))
                return;

            try
            {
                if (!Settings.Default.IsOpenItemWindowInNewWindowChecked && Utilities.IsWindowOpen<ItemWindow>())
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
                Log.Error(nameof(OpenItemWindow), e);
                var catchItemWindow = new ItemWindow(item);
                catchItemWindow.Show();
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

            foreach (var item in ItemController.Items)
            {
                if (!IsFullItemInfoLoading) break;

                item.FullItemInformation = await ItemController.GetFullItemInformationAsync(item);
                LoadFullItemInfoProBarValue++;
            }

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

        public async void SetGoldChart(int count)
        {
            var goldPriceList = await ApiController.GetGoldPricesFromJsonAsync(null, count).ConfigureAwait(true);

            var date = new List<string>();
            var amount = new ChartValues<int>();

            foreach (var goldPrice in goldPriceList)
            {
                date.Add(goldPrice.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                amount.Add(goldPrice.Price);
            }

            Labels = date.ToArray();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Gold",
                    Values = amount,
                    Fill = (Brush) Application.Current.Resources["Solid.Color.Gold.Fill"],
                    Stroke = (Brush) Application.Current.Resources["Solid.Color.Text.Gold"]
                }
            };
        }

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                _seriesCollection = value;
                OnPropertyChanged();
            }
        }

        public string[] Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged();
            }
        }

        #endregion Gold (Gold Mode)

        #region Tracking Mode

        public void TrackerActivationToggle()
        {
            if (!IsReadyToTracking()) return;

            IsTrackingActive = !IsTrackingActive;

            if (IsTrackingActive)
            {
                StartTracking();
            }
            else
            {
                StopTracking();
            }
        }

        public void StartTracking()
        {
            if (NetworkManager.IsNetworkCaptureRunning)
            {
                return;
            }

            if (_trackingController == null)
            {
                _trackingController = new TrackingController(this, _mainWindow);
            }

            _trackingController?.RegisterEvents();
            _trackingController?.DungeonController?.LoadDungeonFromFile();
            _trackingController?.DungeonController?.SetDungeonStatsDay();
            _trackingController?.DungeonController?.SetDungeonStatsTotal();
            _trackingController?.DungeonController?.SetOrUpdateDungeonDataToUi();

            _trackingController?.CountUpTimer.Start();

            IsTrackingActive = NetworkManager.StartNetworkCapture(this, _trackingController);
            Console.WriteLine(@"### Start Tracking...");
        }

        public void StopTracking()
        {
            _trackingController?.DungeonController?.SaveDungeonsInFile();
            _trackingController?.UnregisterEvents();
            _trackingController?.CountUpTimer?.Stop();

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
            _trackingController?.CountUpTimer?.Reset();
        }

        public void ResetDamageMeter()
        {
            _trackingController.CombatController.ResetDamageMeter();
        }

        public void ResetDungeons()
        {
            _trackingController.DungeonController.ResetDungeons();
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
            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Damage)
            {
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.DamageInPercent).ToList());
                return;
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Dps)
            {
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.Dps).ToList());
                return;
            }

            if (DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Name)
                DamageMeter.OrderByReference(DamageMeter.OrderBy(x => x.Name).ToList());
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
                Log.Error(nameof(SetItemCounterAsync), e);
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

        public Visibility IsMainTrackerPopupVisible {
            get => _isMainTrackerPopupVisible;
            set
            {
                _isMainTrackerPopupVisible = value;
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

        public bool IsTrackingResetByMapChangeActive
        {
            get => _isTrackingResetByMapChangeActive;
            set
            {
                _isTrackingResetByMapChangeActive = value;
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

        public ObservableCollection<TrackingNotification> DebugTrackingNotifications {
            get => _debugTrackingNotifications;
            set
            {
                _debugTrackingNotifications = value;
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

                var trackingIconColorOn = new SolidColorBrush((Color) Application.Current.Resources["Tracking.On"]);
                var trackingIconColorOff = new SolidColorBrush((Color) Application.Current.Resources["Tracking.Off"]);
                TrackingIconColor = _isTrackingActive ? trackingIconColorOn : trackingIconColorOff;
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

        public Brush TrackingIconColor
        {
            get => _trackerActivationToggleColor ?? new SolidColorBrush((Color) Application.Current.Resources["Tracking.Off"]);
            set
            {
                _trackerActivationToggleColor = value;
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

        public string FullItemInformationExistLocal
        {
            get => _fullItemInformationExistLocal;
            set
            {
                _fullItemInformationExistLocal = value;
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
                Settings.Default.SavedPlayerInformationName = _savedPlayerInformationName;
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

        public string DonateUrl => Settings.Default.DonateUrl;
        public string DiscordUrl => Settings.Default.DiscordUrl;
        public string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;
        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

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