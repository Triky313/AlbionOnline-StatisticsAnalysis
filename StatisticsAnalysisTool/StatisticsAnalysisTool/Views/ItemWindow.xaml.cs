namespace StatisticsAnalysisTool.Views
{
    using Models;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using ViewModels;

    /// <summary>
    ///     Interaktionslogik für ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow
    {

        private readonly ItemWindowViewModel _itemWindowViewModel;

        public ItemWindow(Item item)
        {
            InitializeComponent();
            _itemWindowViewModel = new ItemWindowViewModel(this, item);
            DataContext = _itemWindowViewModel;
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.RunUpdate = false;
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ShowVillagesPrices_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }

        private void ChbShowBlackZoneOutposts_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
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

        private void BtnErrorBar_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.ErrorBarVisibility = Visibility.Hidden;
        }

        private void CbNormalQuality_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }
        private void CbGoodQuality_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }
        private void CbCbOutstandingQuality_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }
        private void CbExcellentQuality_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }
        private void CbMasterpieceQuality_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.GetPriceStats();
            _itemWindowViewModel.SetHistoryChart();
        }
        
        private void ImageAwesome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _itemWindowViewModel.IsAutoUpdateActive = !_itemWindowViewModel.IsAutoUpdateActive;
            _itemWindowViewModel.RefreshSpin = _itemWindowViewModel.IsAutoUpdateActive;
        }
    }
}