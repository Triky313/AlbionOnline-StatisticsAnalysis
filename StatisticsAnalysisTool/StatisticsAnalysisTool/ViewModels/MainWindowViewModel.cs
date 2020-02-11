using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels
{
    using Annotations;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Views;

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

        public enum ViewMode
        {
            Normal,
            Player,
            Gold
        }

        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Utilities.AutoUpdate();
            UpgradeSettings();
            InitLanguage();
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

        private void InitLanguage()
        {
            if (LanguageController.SetFirstLanguageIfPossible())
                return;

            MessageBox.Show("ERROR: No language file found!");
            _mainWindow.Close();
        }

        private void InitMainWindowData()
        {
            #region Set combobox mode

            Modes.Clear();
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("NORMAL"), ViewMode = ViewMode.Normal });
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("PLAYER"), ViewMode = ViewMode.Player });
            Modes.Add(new ModeStruct { Name = LanguageController.Translation("GOLD"), ViewMode = ViewMode.Gold });
            ModeSelection = Modes.FirstOrDefault(x => x.ViewMode == ViewMode.Normal);

            #endregion

            Task.Run(async () =>
            {
                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    _mainWindow.TxtSearch.IsEnabled = false;
                    _mainWindow.FaLoadIcon.Visibility = Visibility.Visible;
                    
                    #region Set MainWindow height and width and center window

                    _mainWindow.Height = Settings.Default.MainWindowHeight;
                    _mainWindow.Width = Settings.Default.MainWindowWidth;
                    if (Settings.Default.MainWindowMaximized)
                    {
                        _mainWindow.WindowState = WindowState.Maximized;
                    }

                    CenterWindowOnScreen();
                    #endregion

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
                        _mainWindow.TxtSearch.IsEnabled = true;
                        _mainWindow.TxtSearch.Focus();
                    }
                });

                // TEST
                var t = await ApiController.GetGoldPricesFromJsonAsync(null, 1).ConfigureAwait(false);
                CurrentGoldPrice = t.FirstOrDefault()?.Price ?? 0;
            });
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

        // Info Link -> https://github.com/broderickhyman/ao-bin-dumps
        // Models: https://github.com/broderickhyman/albiondata-models-dotNet

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
            var url = Settings.Default.CurrentItemListSourceUrl;
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
            if (string.IsNullOrEmpty(Settings.Default.CurrentItemListSourceUrl))
            {
                url = Settings.Default.DefaultItemListSourceUrl;
                if (string.IsNullOrEmpty(url))
                    return false;

                Settings.Default.CurrentItemListSourceUrl = Settings.Default.DefaultItemListSourceUrl;
                MessageBox.Show(LanguageController.Translation("DEFAULT_ITEMLIST_HAS_BEEN_LOADED"), LanguageController.Translation("NOTE"));
            }
            return true;
        }

        public void LoadLvItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            _mainWindow.Dispatcher?.InvokeAsync(async () =>
            {
                var items = await FindItemsAsync(searchText);
                FilteredItems = items;
                _mainWindow.LblItemCounter.Content = $"{items.Count}/{FilteredItems.Count}";
                _mainWindow.LblLocalImageCounter.Content = ImageController.LocalImagesCounter();
            });
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
            var gameInfoPlayers = await ApiController.GetGameInfoPlayersFromJsonAsync(gameInfoSearch?.SearchPlayer?.FirstOrDefault()?.Id);

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

        public PlayerModeTranslation PlayerModeTranslation => new PlayerModeTranslation();
        public string DonateUrl => Settings.Default.DonateUrl;
        public string SavedPlayerInformationName => Settings.Default.SavedPlayerInformationName ?? "";
        public string LoadTranslation => LanguageController.Translation("LOAD");
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