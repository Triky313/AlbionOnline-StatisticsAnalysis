using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for DamageMeterWindow.xaml
/// </summary>
public partial class DamageMeterWindow
{
    private readonly WindowChromeController _windowChromeController;

    public DamageMeterWindow(DamageMeterBindings damageMeterBindings)
    {
        InitializeComponent();
        _windowChromeController = new WindowChromeController(
            this,
            MaximizedButton,
            ResizeMode.CanResizeWithGrip,
            centerOnRestore: true);
        DataContext = new DamageMeterWindowViewModel(damageMeterBindings);
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
}
