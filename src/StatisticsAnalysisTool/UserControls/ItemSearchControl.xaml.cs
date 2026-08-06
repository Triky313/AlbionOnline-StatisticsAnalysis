using StatisticsAnalysisTool.ViewModels;
using System.Windows;
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

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        var maximumItemListWidth = ItemSearchLayoutGrid.ActualWidth - ItemSearchSplitterColumn.ActualWidth - ItemDetailsColumn.MinWidth;
        ItemListColumn.MaxWidth = maximumItemListWidth >= ItemListColumn.MinWidth
            ? maximumItemListWidth
            : ItemListColumn.MinWidth;
    }

    #region Ui events

    private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm?.ItemFilterReset();
    }

    private void AlertModeAlertActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm?.ToggleAlertSender(sender);
    }

    private void OpenItemSearchInfoPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.IsItemSearchPopupVisible = Visibility.Visible;
    }

    private void CloseItemSearchInfoPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.IsItemSearchPopupVisible = Visibility.Hidden;
    }

    #endregion
}