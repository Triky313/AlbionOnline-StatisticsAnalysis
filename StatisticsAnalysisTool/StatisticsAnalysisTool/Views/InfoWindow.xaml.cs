using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.Views
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow
    {
        private readonly InfoWindowViewModel _infoWindowViewModel;

        public InfoWindow()
        {
            InitializeComponent();
            _infoWindowViewModel = new InfoWindowViewModel(this);
            DataContext = _infoWindowViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
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

        private void ChbShowNotAgain_Click(object sender, RoutedEventArgs e)
        {
            _infoWindowViewModel.SaveShowNotAgainSetting();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
