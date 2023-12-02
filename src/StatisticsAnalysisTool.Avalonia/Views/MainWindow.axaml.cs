using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Avalonia.ViewModels;

namespace StatisticsAnalysisTool.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.ServiceProvider?.GetRequiredService<MainWindowViewModel>();
        _mainWindowViewModel = DataContext as MainWindowViewModel;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        _mainWindowViewModel?.SetWindowSettings(Position);
    }

    private void TopBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }

        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
