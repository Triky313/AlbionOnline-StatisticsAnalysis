using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace StatisticsAnalysisTool.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktopLifetime)
        {
            //SettingsController.SetWindowSettings(desktopLifetime.MainWindow.WindowState, Height, Width, desktopLifetime.MainWindow.Position);
        }
    }
}
