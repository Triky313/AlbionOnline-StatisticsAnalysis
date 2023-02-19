using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for DialogWindow.xaml
/// </summary>
public partial class DialogWindow
{
    public readonly DialogWindowViewModel DialogWindowViewModel;

    public DialogWindow(string title, string message, DialogType type = DialogType.YesNo)
    {
        InitializeComponent();

        DialogWindowViewModel = new DialogWindowViewModel(title, message, type);
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