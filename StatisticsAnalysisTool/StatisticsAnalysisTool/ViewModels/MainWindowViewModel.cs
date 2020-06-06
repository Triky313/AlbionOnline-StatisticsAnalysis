using LiveCharts;
using Newtonsoft.Json;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels
{
    using LiveCharts.Wpf;
    using System.Windows.Media;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static MainWindow _mainWindow;

        private static List<Item> _items;
        private static List<Item> _filteredItems;
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
            UpdateTranslation = LanguageController.Translation("UPDATE");
            NumberOfValuesTranslation = LanguageController.Translation("NUMBER_OF_VALUES");
            LoadTranslation = LanguageController.Translation("LOAD");

            SetModeCombobox();
            ItemController.GetItemInformationListFromLocalAsync();

            var currentGoldPrice = await ApiController.GetGoldPricesFromJsonAsync(null, 1).ConfigureAwait(true);
            CurrentGoldPrice = currentGoldPrice.FirstOrDefault()?.Price ?? 0;
            CurrentGoldPriceTimestamp = currentGoldPrice.FirstOrDefault()?.Timestamp.ToString(CultureInfo.CurrentCulture) ?? new DateTime(0, 0, 0, 0, 0, 0).ToString(CultureInfo.CurrentCulture);

            await Task.Run(async () =>
            {
                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    IsTxtSearchEnabled = false;
                    _mainWindow.FaLoadIcon.Visibility = Visibility.Visible;

                    _mainWindow.TxtBoxPlayerModeUsername.Text = Settings.Default.SavedPlayerInformationName;
                });
                
                var isItemListLoaded = await GetItemListFromJsonAsync().ConfigureAwait(true);
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

        public List<Item> FilteredItems {
            get => _filteredItems;
            set {
                _filteredItems = value;
                OnPropertyChanged();
            }
        }

        public async Task<List<Item>> FindItemsAsync(string searchText)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return _items?.FindAll(s => (s.LocalizedNameAndEnglish.ToLower().Contains(searchText.ToLower())));
                }
                catch
                {
                    return null;
                }
            });
        }

        public async Task<bool> GetItemListFromJsonAsync()
        {
            var url = Settings.Default.ItemListSourceUrl;
            if (!GetItemListSourceUrlIfExist(ref url))
                return false;

            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}"))
            {
                var fileDateTime = File.GetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");

                if (fileDateTime.AddDays(7) < DateTime.Now)
                {
                    _items = await TryToGetItemListFromWeb(url);
                    return (_items != null);
                }

                _items = GetItemListFromLocal();
                return (_items != null);
            }

            _items = await TryToGetItemListFromWeb(url);
            return (_items != null);
        }

        private static List<Item> GetItemListFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}");
                return JsonConvert.DeserializeObject<List<Item>>(localItemString);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<List<Item>> TryToGetItemListFromWeb(string url)
        {
            using (var wd = new WebDownload(30000))
            {
                try
                {
                    var itemsString = await wd.DownloadStringTaskAsync(url);
                    File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<List<Item>>(itemsString);
                }
                catch (Exception)
                {
                    try
                    {
                        var itemsString = await wd.DownloadStringTaskAsync(Settings.Default.DefaultItemListSourceUrl);
                        File.WriteAllText($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ItemListFileName}", itemsString, Encoding.UTF8);
                        return JsonConvert.DeserializeObject<List<Item>>(itemsString);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        private static bool GetItemListSourceUrlIfExist(ref string url)
        {
            if (string.IsNullOrEmpty(Settings.Default.ItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                    return false;

                Settings.Default.ItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }
            return true;
        }

        public async void LoadLvItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            var items = await FindItemsAsync(searchText);
            FilteredItems = items;

            _mainWindow.Dispatcher?.InvokeAsync(async () =>
            {
                LocalImageCounter = await ImageController.LocalImagesCounterAsync();
            });

            ItemCounterString = $"{items.Count}/{items.Count}";
        }

        #endregion

        #region Player information (Player Mode)

        public async Task SetComparedPlayerModeInfoValues()
        {
            PlayerModeInformationLocal = PlayerModeInformation;
            PlayerModeInformation = new PlayerModeInformationModel();
            PlayerModeInformation = await GetPlayerModeInformationByApi().ConfigureAwait(true);
        }
        
        private static async Task<PlayerModeInformationModel> GetPlayerModeInformationByApi()
        {
            if (string.IsNullOrWhiteSpace(_mainWindow.TxtBoxPlayerModeUsername.Text))
                return null;

            var gameInfoSearch = await ApiController.GetGameInfoSearchFromJsonAsync(_mainWindow.TxtBoxPlayerModeUsername.Text);
            
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

        public string NumberOfValuesTranslation {
            get => _numberOfValuesTranslation;
            set {
                _numberOfValuesTranslation = value;
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

        public string DonateUrl => Settings.Default.DonateUrl;
        public string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;
        public string SavedPlayerInformationName => Settings.Default.SavedPlayerInformationName ?? "";

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