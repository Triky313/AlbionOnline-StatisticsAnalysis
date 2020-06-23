using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow
    {
        public InfoWindow()
        {
            InitializeComponent();
            var infoWindowViewModel = new InfoWindowViewModel();
            DataContext = infoWindowViewModel;
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
    }
}
