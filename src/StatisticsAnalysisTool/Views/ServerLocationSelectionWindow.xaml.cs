using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;
/// <summary>
/// Interaction logic for GameDataPreparationWindow.xaml
/// </summary>
public partial class ServerLocationSelectionWindow
{
    private readonly ServerLocationSelectionWindowViewModel _serverLocationSelectionWindowViewModel;

    public ServerLocationSelectionWindow()
    {
        InitializeComponent();
        _serverLocationSelectionWindowViewModel = new ServerLocationSelectionWindowViewModel();
        DataContext = _serverLocationSelectionWindowViewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (_serverLocationSelectionWindowViewModel.SelectedServerLocation.ServerLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
        {
            return;
        }

        SettingsController.CurrentSettings.ServerLocation = _serverLocationSelectionWindowViewModel.SelectedServerLocation.ServerLocation;
        DialogResult = true;
        Close();
    }
}
