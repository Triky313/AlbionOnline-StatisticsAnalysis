using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ItemSearchControl.xaml
/// </summary>
public partial class ItemSearchControl
{
    public ItemSearchControl()
    {
        InitializeComponent();
    }

    #region Ui events

    private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var item = (Item) ((ListView) sender).SelectedValue;
        MainWindowViewModelOld.OpenItemWindow(item);
    }

    private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        vm?.ItemFilterReset();
    }

    private void AlertModeAlertActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        vm?.ToggleAlertSender(sender);
    }

    #endregion
}