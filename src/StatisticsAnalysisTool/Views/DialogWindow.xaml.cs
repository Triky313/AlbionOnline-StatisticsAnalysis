using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for DialogWindow.xaml
/// </summary>
public partial class DialogWindow
{
    public readonly DialogWindowViewModel DialogWindowViewModel;

    public DialogWindow(string title, string message, DialogType type = DialogType.YesNo) : this(title, message, type, null)
    {
    }

    public DialogWindow(string title, string message, DialogType type, string url, string urlText = null)
    {
        InitializeComponent();

        DialogWindowViewModel = new DialogWindowViewModel(title, message, type, url, urlText);
        DataContext = DialogWindowViewModel;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogWindowViewModel.Canceled = true;
        DialogResult = false;
        Close();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        DialogWindowViewModel.Canceled = false;
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogWindowViewModel.Canceled = true;
        DialogResult = false;
        Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        });

        e.Handled = true;
    }

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

        if (e.ClickCount == 2 && WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
    }
}