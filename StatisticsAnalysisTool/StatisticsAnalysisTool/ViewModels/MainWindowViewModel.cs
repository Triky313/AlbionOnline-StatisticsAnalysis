using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static MainWindow _mainWindow;

        private static PlayerModeInformationModel _playerModeInformationLocal;
        private static PlayerModeInformationModel _playerModeInformation;

        private static readonly string PlayerModeUpdateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.PlayerModeUpdateFileName);

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
            Task.Run(async () =>
            {
                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    _mainWindow.TxtSearch.IsEnabled = false;
                    _mainWindow.FaLoadIcon.Visibility = Visibility.Visible;

                    #region Set combobox mode

                    _mainWindow.CbMode.Items.Clear();
                    _mainWindow.CbMode.Items.Add(new ComboboxMarketMode { Name = LanguageController.Translation("NORMAL"), Mode = MainWindow.ViewMode.Normal });
                    _mainWindow.CbMode.Items.Add(new ComboboxMarketMode { Name = LanguageController.Translation("PLAYER"), Mode = MainWindow.ViewMode.Player });

                    if (_mainWindow.CbMode.Items.Count > 0)
                        _mainWindow.CbMode.SelectedIndex = 0;

                    #endregion

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
                
                var isItemListLoaded = await StatisticsAnalysisManager.GetItemListFromJsonAsync().ConfigureAwait(true);
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

        public void LoadLvItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            _mainWindow.Dispatcher?.InvokeAsync(async () =>
            {
                var items = await StatisticsAnalysisManager.FindItemsAsync(searchText);
                _mainWindow.LvItems.ItemsSource = items;
                _mainWindow.LblItemCounter.Content = $"{items.Count}/{StatisticsAnalysisManager.Items.Count}";
                _mainWindow.LblLocalImageCounter.Content = ImageController.LocalImagesCounter();
            });
        }
        
        public async Task SetComparedPlayerModeInfoValues()
        {
            PlayerModeInformationLocal = PlayerModeInformation;
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

        public PlayerModeTranslation PlayerModeTranslation => new PlayerModeTranslation();
        public string DonateUrl => Settings.Default.DonateUrl;
        public string SavedPlayerInformationName => Settings.Default.SavedPlayerInformationName ?? "";
        public string SaveTranslation => LanguageController.Translation("SAVE");
        public string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}