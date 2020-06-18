using LiveCharts;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
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

namespace StatisticsAnalysisTool.ViewModels
{
    using LiveCharts.Wpf;
    using System.Windows.Media;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static MainWindow _mainWindow;

        private static List<Item> _itemListViewItemsSource;
        private static PlayerModeInformationModel _playerModeInformationLocal;
        private static PlayerModeInformationModel _playerModeInformation;
        private ObservableCollection<ModeStruct> _modes = new ObservableCollection<ModeStruct>();
        private ModeStruct _modeSelection;
        private int _currentGoldPrice;
        private string _currentGoldPriceTimestamp;
        private SeriesCollection _seriesCollection;
        private string[] _labels;
        private string _textBoxGoldModeNumberOfValues;
        private string _updateTranslation;
        private string _numberOfValuesTranslation;
        private string _loadTranslation;
        private PlayerModeTranslation _playerModeTranslation = new PlayerModeTranslation();
        private bool _isTxtSearchEnabled;
        private string _itemCounterString;
        private int _localImageCounter;
        private string _fullItemInformationExistLocal;
        private Dictionary<Category, string> _itemCategories;
        private Category _selectedItemCategories;
        private Dictionary<ParentCategory, string> _itemParentCategories;
        private ParentCategory _selectedItemParentCategories;
        private Dictionary<ItemTier, string> _itemTiers;
        private ItemTier _selectedItemTier;
        private Dictionary<ItemLevel, string> _itemLevels;
        private ItemLevel _selectedItemLevel;
        private string _searchText;
        private bool _isFullItemInfoSearch;
        private Visibility _itemLevelsVisibility;
        private Visibility _itemTiersVisibility;
        private Visibility _itemCategoriesVisibility;
        private Visibility _itemParentCategoriesVisibility;
        private MainWindowTranslation _translation;
        private string _savedPlayerInformationName;

        public enum ViewMode
        {
            Normal,
            Player,
            Gold
        }

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitWindowSettings();
            Utilities.AutoUpdate();
            UpgradeSettings();

            if (!LanguageController.InitializeLanguage())
                _mainWindow.Close();
            
            InitMainWindowData();
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
                if (Settings.Default.MainWindowMaximized)
                {
                    _mainWindow.WindowState = WindowState.Maximized;
                }

                CenterWindowOnScreen();
                #endregion
            });
        }

        private async void InitMainWindowData()
        {
            Translation = new MainWindowTranslation();

            SetModeCombobox();
            ItemController.GetItemInformationListFromLocalAsync();

            IsFullItemInfoSearch = Settings.Default.IsFullItemInfoSearch;
            ItemParentCategories = CategoryController.ParentCategoryNames;
            ItemTiers = FrequentlyValues.ItemTiers;
            SelectedItemTier = ItemTier.Unknown;
            ItemLevels = FrequentlyValues.ItemLevels;
            SelectedItemLevel = ItemLevel.Unknown;
            
            var currentGoldPrice = await ApiController.GetGoldPricesFromJsonAsync(null, 1).ConfigureAwait(true);
            CurrentGoldPrice = currentGoldPrice.FirstOrDefault()?.Price ?? 0;
            CurrentGoldPriceTimestamp = currentGoldPrice.FirstOrDefault()?.Timestamp.ToString(CultureInfo.CurrentCulture) ?? new DateTime(0, 0, 0, 0, 0, 0).ToString(CultureInfo.CurrentCulture);

            await Task.Run(async () =>
            {
                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    IsTxtSearchEnabled = false;
                    _mainWindow.FaLoadIcon.Visibility = Visibility.Visible;

                    SavedPlayerInformationName = Settings.Default.SavedPlayerInformationName;
                });
                
                var isItemListLoaded = await ItemController.GetItemListFromJsonAsync().ConfigureAwait(true);
                if (!isItemListLoaded)
                    MessageBox.Show(LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"),
                        LanguageController.Translation("ERROR"));

                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    if (isItemListLoaded)
                    {
                        _mainWindow.FaLoadIcon.Visibility = Visibility.Hidden;
                        IsTxtSearchEnabled = true;
                        _mainWindow.TxtSearch.Focus();
                    }
                });
            });

            ShowInfoWindow();
            TextBoxGoldModeNumberOfValues = "10";
        }

        public void SetModeCombobox()
        {
            Modes.Clear();
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("NORMAL"), ViewMode = ViewMode.Normal });
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("PLAYER"), ViewMode = ViewMode.Player });
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("GOLD"), ViewMode = ViewMode.Gold });
            ModeSelection = Modes.FirstOrDefault(x => x.ViewMode == ViewMode.Normal);
        }

        public void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = _mainWindow.Width;
            var windowHeight = _mainWindow.Height;
            _mainWindow.Left = (screenWidth / 2) - (windowWidth / 2);
            _mainWindow.Top = (screenHeight / 2) - (windowHeight / 2);
        }
        
        #region Item list (Normal Mode)
        
        private List<Item> GetFilteredItemList(string searchText)
        {
            var filteredItemList = new List<Item>();

            if (ItemController.Items == null)
            {
                return filteredItemList;
            }

            if (IsFullItemInfoSearch)
            {
                filteredItemList = ItemController.Items.Where(x =>
                    x?.FullItemInformationFromLocal != null &&
                    x.LocalizedNameAndEnglish.ToLower().Contains(searchText?.ToLower() ?? string.Empty)
                    && (x.FullItemInformationFromLocal.CategoryObject.ParentCategory == SelectedItemParentCategory || SelectedItemParentCategory == ParentCategory.Unknown)
                    && (x.FullItemInformationFromLocal.CategoryObject.Category == SelectedItemCategory || SelectedItemCategory == Category.Unknown)
                    && ((ItemTier)x.FullItemInformationFromLocal.Tier == SelectedItemTier || SelectedItemTier == ItemTier.Unknown)
                    && ((ItemLevel)x.FullItemInformationFromLocal.Level == SelectedItemLevel || SelectedItemLevel == ItemLevel.Unknown)).ToList();
            }
            else
            {
                filteredItemList = ItemController.Items.Where(x => x.LocalizedNameAndEnglish.ToLower().Contains(searchText?.ToLower() ?? string.Empty)).ToList();
            }

            SetItemCounterAsync(filteredItemList.Count);

            return filteredItemList;
        }

        private async void SetItemCounterAsync(int? items)
        {
            LocalImageCounter = await ImageController.LocalImagesCounterAsync();
            ItemCounterString = $"{items ?? 0}/{ItemController.Items?.Count ?? 0}";
        }

        public List<Item> ItemListViewItemsSource {
            get => _itemListViewItemsSource;
            set {
                _itemListViewItemsSource = value;
                OnPropertyChanged();
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

            return new PlayerModeInformationModel() 
            { 
                Timestamp = DateTime.UtcNow,
                GameInfoSearch = gameInfoSearch,
                SearchPlayer = searchPlayer,
                GameInfoPlayers = gameInfoPlayers
            };
        }

        public PlayerModeInformationModel PlayerModeInformation {
            get => _playerModeInformation;
            set {
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

        #endregion

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
                    Fill = (Brush)Application.Current.Resources["Solid.Color.Gold.Fill"],
                    Stroke = (Brush)Application.Current.Resources["Solid.Color.Text.Gold"]
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
        
        #endregion

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
            if (string.IsNullOrEmpty(item.UniqueName))
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
            catch (ArgumentNullException)
            {
                var catchItemWindow = new ItemWindow(item);
                catchItemWindow.Show();
            }
        }

        public string SearchText {
            get => _searchText;
            set
            {
                _searchText = value;
                ItemListViewItemsSource = GetFilteredItemList(_searchText);
                OnPropertyChanged();
            }
        }

        public bool IsFullItemInfoSearch {
            get => _isFullItemInfoSearch;
            set
            {
                _isFullItemInfoSearch = value;

                if (_isFullItemInfoSearch)
                {
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Visible;
                }
                else
                {
                    ItemLevelsVisibility = ItemTiersVisibility = ItemCategoriesVisibility = ItemParentCategoriesVisibility = Visibility.Hidden;
                }

                ItemListViewItemsSource = GetFilteredItemList(_searchText);
                Settings.Default.IsFullItemInfoSearch = _isFullItemInfoSearch;
                OnPropertyChanged();
            }
        }

        public Visibility ItemLevelsVisibility {
            get => _itemLevelsVisibility;
            set {
                _itemLevelsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemTiersVisibility {
            get => _itemTiersVisibility;
            set {
                _itemTiersVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemCategoriesVisibility {
            get => _itemCategoriesVisibility;
            set {
                _itemCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ItemParentCategoriesVisibility {
            get => _itemParentCategoriesVisibility;
            set
            {
                _itemParentCategoriesVisibility = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<Category, string> ItemCategories {
            get => _itemCategories;
            set
            {
                var categories = value;
                categories = (new Dictionary<Category, string> { { Category.Unknown, string.Empty } }).Concat(categories).ToDictionary(k => k.Key, v => v.Value);
                _itemCategories = categories;
                OnPropertyChanged();
            }
        }
        
        public Category SelectedItemCategory {
            get => _selectedItemCategories;
            set {
                _selectedItemCategories = value;
                ItemListViewItemsSource = GetFilteredItemList(SearchText);
                OnPropertyChanged();
            }
        }

        public Dictionary<ParentCategory, string> ItemParentCategories {
            get => _itemParentCategories;
            set {
                _itemParentCategories = value;
                OnPropertyChanged();
            }
        }

        public ParentCategory SelectedItemParentCategory {
            get => _selectedItemParentCategories;
            set {
                _selectedItemParentCategories = value;
                ItemCategories = CategoryController.GetCategoriesByParentCategory(SelectedItemParentCategory);
                SelectedItemCategory = Category.Unknown;
                ItemListViewItemsSource = GetFilteredItemList(SearchText);
                OnPropertyChanged();
            }
        }

        public Dictionary<ItemTier, string> ItemTiers {
            get => _itemTiers;
            set {
                _itemTiers = value;
                OnPropertyChanged();
            }
        }

        public ItemTier SelectedItemTier {
            get => _selectedItemTier;
            set {
                _selectedItemTier = value;
                ItemListViewItemsSource = GetFilteredItemList(SearchText);
                OnPropertyChanged();
            }
        }

        public Dictionary<ItemLevel, string> ItemLevels {
            get => _itemLevels;
            set {
                _itemLevels = value;
                OnPropertyChanged();
            }
        }

        public ItemLevel SelectedItemLevel {
            get => _selectedItemLevel;
            set {
                _selectedItemLevel = value;
                ItemListViewItemsSource = GetFilteredItemList(SearchText);
                OnPropertyChanged();
            }
        }
        
        public int LocalImageCounter {
            get => _localImageCounter;
            set {
                _localImageCounter = value;
                OnPropertyChanged();
            }
        }

        public string ItemCounterString {
            get => _itemCounterString;
            set {
                _itemCounterString = value;
                OnPropertyChanged();
            }
        }

        public bool IsTxtSearchEnabled {
            get => _isTxtSearchEnabled;
            set {
                _isTxtSearchEnabled = value;
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
        
        public PlayerModeTranslation PlayerModeTranslation {
            get => _playerModeTranslation;
            set {
                _playerModeTranslation = value;
                OnPropertyChanged();
            }
        }

        public string LoadTranslation {
            get => _loadTranslation;
            set {
                _loadTranslation = value;
                OnPropertyChanged();
            }
        }

        public string FullItemInformationExistLocal {
            get => _fullItemInformationExistLocal;
            set {
                _fullItemInformationExistLocal = value;
                OnPropertyChanged();
            }
        }

        public string NumberOfValuesTranslation {
            get => _numberOfValuesTranslation;
            set {
                _numberOfValuesTranslation = value;
                OnPropertyChanged();
            }
        }

        public string SavedPlayerInformationName {
            get => _savedPlayerInformationName;
            set {
                _savedPlayerInformationName = value;
                Settings.Default.SavedPlayerInformationName = _savedPlayerInformationName;
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

        public MainWindowTranslation Translation {
            get => _translation;
            set {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public string DonateUrl => Settings.Default.DonateUrl;
        public string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;
        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public struct ModeStruct
        {
            public string Name { get; set; }
            public ViewMode ViewMode { get; set; }
        }
    }
}