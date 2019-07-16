using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using FontAwesome.WPF;
using MarketAnalysisTool.Models;
using MarketAnalysisTool.Properties;
using MarketAnalysisTool.Utilities;

namespace MarketAnalysisTool
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public enum MarketMode
        {
            Normal
        }
        
        private readonly IniFile _iniFile =
            new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SettingsFileName));

        //private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        //private GridViewColumnHeader _lastHeaderClicked;

        public MainWindow()
        {
            InitializeComponent();
            Utility.AutoUpdate();
            InitMarketAnalysis();
        }

        private void InitMarketAnalysis()
        {
            Task.Run(async () =>
            {
                Dispatcher.Invoke(() =>
                {
                    InitLanguage();
                    TxtSearch.IsEnabled = false;
                    FaLoadIcon.Visibility = Visibility.Visible;
                    InitUi();
                });

                await StatisticsAnalysisManager.GetItemsFromJsonAsync();

                #region Refrash rate

                if (_iniFile.SectionKeyExists("Settings", "RefreshRate") &&
                    int.TryParse(_iniFile.ReadValue("Settings", "RefreshRate"), out var refrashrate))
                    StatisticsAnalysisManager.RefreshRate = refrashrate;

                #endregion

                #region Update item list by days

                if (_iniFile.SectionKeyExists("Settings", "UpdateItemListByDays") &&
                    int.TryParse(_iniFile.ReadValue("Settings", "UpdateItemListByDays"), out var updateItemListByDays))
                    StatisticsAnalysisManager.UpdateItemListByDays = updateItemListByDays;

                #endregion

                Dispatcher.Invoke(() =>
                {
                    FaLoadIcon.Visibility = Visibility.Hidden;
                    TxtSearch.IsEnabled = true;
                });
            });
        }

        private void InitLanguage()
        {
            // TODO: Überarbeiten und mit in LanguageController einbinden
            LanguageController.InitializeLanguageFiles();

            if (_iniFile.SectionKeyExists("Settings", "Language") &&
                LanguageController.SetLanguage(_iniFile.ReadValue("Settings", "Language")))
            {
            }
            else
            {
                if (!LanguageController.SetLanguage(LanguageController.FileInfos.FirstOrDefault()?.FileName))
                {
                    MessageBox.Show("ERROR: No language file found!");
                    Close();
                }
            }
        }

        private void InitUi()
        {
            // Title
            LblToolName.Content =
                $"AlbionOnline - STATISTICS ANALYSIS TOOL | v{Assembly.GetExecutingAssembly().GetName().Version}";

            CbMode.Items.Clear();
            if (IsModeActive(MarketMode.Normal))
                CbMode.Items.Add(new ComboboxMarketMode
                    {Name = LanguageController.Translation("NORMAL"), Mode = MarketMode.Normal});
            if (CbMode.Items.Count > 0)
                CbMode.SelectedIndex = 0;
        }

        public void LoadLvItems(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            Dispatcher.InvokeAsync(async () =>
            {
                var items = await StatisticsAnalysisManager.FindItemsAsync(searchText);
                LvItems.ItemsSource = items;
                LblItemCounter.Content = $"{items.Count}/{StatisticsAnalysisManager.Items.Count}";
            });
        }
        
        public bool IsModeActive(MarketMode mode)
        {
            var settingModes = Settings.Default.ActiveMode.Split(',');

            switch (mode)
            {
                case MarketMode.Normal:
                    return settingModes.Contains("Normal");
                default:
                    return false;
            }
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

        private void CbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ComboboxMarketMode) CbMode.SelectedItem;

            switch (mode?.Mode)
            {
                case MarketMode.Normal:
                    GridNormalMode.Visibility = Visibility.Visible;
                    return;
            }
        }

        //private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        //{
        //    if (e.OriginalSource is GridViewColumnHeader headerClicked)
        //        if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
        //        {
        //            ListSortDirection direction;
        //            if (headerClicked != _lastHeaderClicked)
        //                direction = ListSortDirection.Ascending;
        //            else
        //                direction = _lastDirection == ListSortDirection.Ascending
        //                    ? ListSortDirection.Descending : ListSortDirection.Ascending;

        //            var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
        //            var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

        //            Sort(sortBy, direction);

        //            if (direction == ListSortDirection.Ascending)
        //                headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate;
        //            else
        //                headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate;

        //            // Remove arrow from previously sorted header
        //            if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
        //                _lastHeaderClicked.Column.HeaderTemplate = null;

        //            _lastHeaderClicked = headerClicked;
        //            _lastDirection = direction;
        //        }
        //}

        //private void Sort(string sortBy, ListSortDirection direction)
        //{
        //    var dataView = CollectionViewSource.GetDefaultView(LvItems.ItemsSource);

        //    if (dataView == null)
        //        return;

        //    try
        //    {
        //        dataView.SortDescriptions.Clear();
        //        var sd = new SortDescription(sortBy, direction);
        //        dataView.SortDescriptions.Add(sd);
        //        dataView.Refresh();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        Debug.Print(ex.Message);
        //    }
        //}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

    }
}