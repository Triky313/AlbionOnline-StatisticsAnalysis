using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for StorageHistoryControl.xaml
/// </summary>
public partial class StorageHistoryControl
{
    public StorageHistoryControl()
    {
        InitializeComponent();
    }

    private void ButtonDeleteCurrentStorage_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_STORAGE"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_STORAGE"));
        var dialogResult = dialog.ShowDialog();

        var vm = (MainWindowViewModel)DataContext;

        if (dialogResult is true)
        {
            vm?.TrackingController?.VaultController?.RemoveVault(vm.VaultBindings?.VaultSelected);
        }
    }
}