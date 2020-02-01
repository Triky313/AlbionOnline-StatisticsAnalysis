using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public enum ViewMode
        {
            Normal,
            Player
        }

        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Utilities.AutoUpdate();
            InitMarketAnalysis();
            _mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = _mainWindowViewModel;
        }

        private void InitMarketAnalysis()
        {
            Task.Run(async () =>
            {
                Dispatcher?.Invoke(() =>
                {
                    TxtSearch.IsEnabled = false;
                    FaLoadIcon.Visibility = Visibility.Visible;
                    InitUi();
                });

                #region Set MainWindow height and width and center window

                Dispatcher?.Invoke(() =>
                {
                    Height = Settings.Default.MainWindowHeight;
                    Width = Settings.Default.MainWindowWidth;
                    if (Settings.Default.MainWindowMaximized)
                    {
                        WindowState = WindowState.Maximized;
                    }

                    CenterWindowOnScreen();
                    TxtBoxPlayerModeUsername.Text = Settings.Default.SavedPlayerInformationName;
                });

                #endregion

                var isItemListLoaded = await StatisticsAnalysisManager.GetItemListFromJsonAsync();
                if (!isItemListLoaded)
                    MessageBox.Show(LanguageController.Translation("ITEM_LIST_CAN_NOT_BE_LOADED"), 
                        LanguageController.Translation("ERROR"));

                Dispatcher?.Invoke(() =>
                {
                    if(isItemListLoaded)
                    {
                        FaLoadIcon.Visibility = Visibility.Hidden;
                        TxtSearch.IsEnabled = true;
                    }
                });
            });
        }

        private void InitUi()
        {
            LblToolName.Content = $"AlbionOnline - STATISTICS ANALYSIS TOOL | v{Assembly.GetExecutingAssembly().GetName().Version}";

            CbMode.Items.Clear();
            CbMode.Items.Add(new ComboboxMarketMode { Name = LanguageController.Translation("NORMAL"), Mode = ViewMode.Normal });
            CbMode.Items.Add(new ComboboxMarketMode { Name = LanguageController.Translation("PLAYER"), Mode = ViewMode.Player });

            if (CbMode.Items.Count > 0)
                CbMode.SelectedIndex = 0;
        }

        public void LoadLvItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            Dispatcher?.InvokeAsync(async () =>
            {
                var items = await StatisticsAnalysisManager.FindItemsAsync(searchText);
                LvItems.ItemsSource = items;
                LblItemCounter.Content = $"{items.Count}/{StatisticsAnalysisManager.Items.Count}";
                LblLocalImageCounter.Content = ImageController.LocalImagesCounter();
            });
        }

        private void CenterWindowOnScreen()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = Width;
            var windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            LoadLvItems(TxtSearch.Text);
        }

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item) ((ListView) sender).SelectedValue;
            var iw = new ItemWindow(item);
            iw.Show();
        }

        private void ImageAwesome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var sw = new SettingsWindow();
            sw.ShowDialog();
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ImageAwesome_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ImageAwesome icon)
                icon.Spin = true;
        }

        private void ImageAwesome_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ImageAwesome icon)
                icon.Spin = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                return;
            }

            if (e.ClickCount == 2 && WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
        }

        private static Rect _storedSize;
        private static bool _isWindowMaximized;

        private void MaximizedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowMaximized)
            {
                Left = _storedSize.Left;
                Top = _storedSize.Top;
                Height = _storedSize.Height;
                Width = _storedSize.Width;
                CenterWindowOnScreen();
                MaximizedButton.Content = 1;
                _isWindowMaximized = false;
            }
            else
            {
                _storedSize.Width = Width;
                _storedSize.Height = Height;
                Left = SystemParameters.WorkArea.Left;
                Top = SystemParameters.WorkArea.Top;
                Height = SystemParameters.WorkArea.Height;
                Width = SystemParameters.WorkArea.Width;
                MaximizedButton.Content = 2;
                _isWindowMaximized = true;
            }
        }

        private void CbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ComboboxMarketMode) CbMode.SelectedItem;

            switch (mode?.Mode)
            {
                case ViewMode.Normal:
                    HideAllGrids();
                    GridNormalMode.Visibility = Visibility.Visible;
                    return;
                case ViewMode.Player:
                    HideAllGrids();
                    GridPlayerMode.Visibility = Visibility.Visible;
                    return;
            }
        }

        private void HideAllGrids()
        {
            GridNormalMode.Visibility = Visibility.Hidden;
            GridPlayerMode.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Settings.Default.MainWindowHeight = RestoreBounds.Height;
                Settings.Default.MainWindowWidth = RestoreBounds.Width;
                Settings.Default.MainWindowMaximized = true;
            }
            else
            {
                Settings.Default.MainWindowHeight = Height;
                Settings.Default.MainWindowWidth = Width;
                Settings.Default.MainWindowMaximized = false;
            }

            Settings.Default.SavedPlayerInformationName = TxtBoxPlayerModeUsername.Text;
            Settings.Default.Save();
        }

        private async Task LoadPlayerInformation()
        {
            if (string.IsNullOrWhiteSpace(TxtBoxPlayerModeUsername.Text))
                return;

            _mainWindowViewModel.GameInfoSearch = await ApiController.GetGameInfoSearchFromJsonAsync(TxtBoxPlayerModeUsername.Text);

            if (_mainWindowViewModel.GameInfoSearch?.SearchPlayer?.FirstOrDefault()?.Id == null)
                return;

            _mainWindowViewModel.SearchPlayer = _mainWindowViewModel.GameInfoSearch?.SearchPlayer?.FirstOrDefault();
            _mainWindowViewModel.GameInfoPlayers = await ApiController.GetGameInfoPlayersFromJsonAsync(_mainWindowViewModel.GameInfoSearch?.SearchPlayer?.FirstOrDefault()?.Id);
        }
        
        private async void BtnPlayerModeSave_Click(object sender, RoutedEventArgs e)
        {
            await LoadPlayerInformation();
        }

        private async void TxtBoxPlayerModeUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            await LoadPlayerInformation();
        }
    }
}