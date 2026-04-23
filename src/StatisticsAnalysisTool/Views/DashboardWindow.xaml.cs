using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for DamageMeterWindow.xaml
/// </summary>
public partial class DashboardWindow
{
    private readonly WindowChromeController _windowChromeController;

    public DashboardWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        _windowChromeController = new WindowChromeController(
            this,
            MaximizedButton,
            ResizeMode.CanResizeWithGrip,
            centerOnRestore: true);
        DataContext = mainWindowViewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizedButton_Click(object sender, RoutedEventArgs e)
    {
        _windowChromeController.ToggleMaximize();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _windowChromeController.DragMoveOnMouseDown(e);
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _windowChromeController.ToggleMaximizeOnDoubleClick(e);
    }

    private void DashboardChartRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshDailyChart();
    }

    private void DashboardChartSeriesVisibility_Changed(object sender, RoutedEventArgs e)
    {
        RefreshDailyChart();
    }

    private static void RefreshDailyChart()
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.StatisticController?.UpdateDailyChart(true);
    }
}
