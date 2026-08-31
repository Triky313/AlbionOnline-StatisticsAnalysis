using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ItemSearchControl.xaml
/// </summary>
public partial class ItemSearchControl
{
    private const double DefaultIconColumnWidth = 75;
    private const double DefaultNameColumnWidth = 215;
    private const double DefaultFavoriteColumnWidth = 55;
    private static readonly DependencyPropertyDescriptor ColumnWidthDescriptor =
        DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn));
    private bool _isColumnWidthTrackingActive;

    public ItemSearchControl()
    {
        InitializeComponent();
        Loaded += ItemSearchControl_Loaded;
        Unloaded += ItemSearchControl_Unloaded;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        var maximumItemListWidth = ItemSearchLayoutGrid.ActualWidth - ItemSearchSplitterColumn.ActualWidth - ItemDetailsColumn.MinWidth;
        ItemListColumn.MaxWidth = maximumItemListWidth >= ItemListColumn.MinWidth
            ? maximumItemListWidth
            : ItemListColumn.MinWidth;
    }

    private void ItemSearchControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isColumnWidthTrackingActive)
        {
            return;
        }

        ApplySavedColumnWidths();
        SetColumnWidthTracking(true);
        _isColumnWidthTrackingActive = true;
    }

    private void ItemSearchControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (!_isColumnWidthTrackingActive)
        {
            return;
        }

        SetColumnWidthTracking(false);
        _isColumnWidthTrackingActive = false;
    }

    private void ApplySavedColumnWidths()
    {
        var settings = SettingsController.CurrentSettings;
        ItemIconColumn.Width = GetValidColumnWidth(settings.ItemSearchIconColumnWidth, DefaultIconColumnWidth);
        ItemNameColumn.Width = GetValidColumnWidth(settings.ItemSearchNameColumnWidth, DefaultNameColumnWidth);
        ItemFavoriteColumn.Width = GetValidColumnWidth(settings.ItemSearchFavoriteColumnWidth, DefaultFavoriteColumnWidth);
    }

    private void SetColumnWidthTracking(bool isActive)
    {
        var columns = new[]
        {
            ItemIconColumn,
            ItemNameColumn,
            ItemFavoriteColumn
        };

        foreach (var column in columns)
        {
            if (isActive)
            {
                ColumnWidthDescriptor.AddValueChanged(column, ItemSearchColumnWidthChanged);
            }
            else
            {
                ColumnWidthDescriptor.RemoveValueChanged(column, ItemSearchColumnWidthChanged);
            }
        }
    }

    private void ItemSearchColumnWidthChanged(object sender, EventArgs e)
    {
        var settings = SettingsController.CurrentSettings;
        settings.ItemSearchIconColumnWidth = GetPersistableColumnWidth(ItemIconColumn, DefaultIconColumnWidth);
        settings.ItemSearchNameColumnWidth = GetPersistableColumnWidth(ItemNameColumn, DefaultNameColumnWidth);
        settings.ItemSearchFavoriteColumnWidth = GetPersistableColumnWidth(ItemFavoriteColumn, DefaultFavoriteColumnWidth);
    }

    private static double GetValidColumnWidth(double columnWidth, double defaultWidth)
    {
        return double.IsNaN(columnWidth) || double.IsInfinity(columnWidth) || columnWidth < 0
            ? defaultWidth
            : columnWidth;
    }

    private static double GetPersistableColumnWidth(GridViewColumn column, double defaultWidth)
    {
        return double.IsNaN(column.Width) || double.IsInfinity(column.Width) || column.Width < 0
            ? GetValidColumnWidth(column.ActualWidth, defaultWidth)
            : column.Width;
    }

    #region Ui events

    private async void ItemSearchList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListView itemSearchList || e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        var itemContainer = ItemsControl.ContainerFromElement(itemSearchList, source) as ListViewItem;
        if (itemContainer?.DataContext is not Item item || !ReferenceEquals(itemSearchList.SelectedItem, item))
        {
            return;
        }

        var rowPresenter = FindVisualParent<GridViewRowPresenter>(source);
        if (rowPresenter == null || e.GetPosition(rowPresenter).X > ItemIconColumn.ActualWidth + ItemNameColumn.ActualWidth)
        {
            return;
        }

        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.RefreshSelectedItemOnOpeningAsync(item);
        }
    }

    internal static T FindVisualParent<T>(DependencyObject source) where T : DependencyObject
    {
        var current = source;

        while (current != null)
        {
            if (current is T parent)
            {
                return parent;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static DependencyObject GetParent(DependencyObject source)
    {
        if (source is ContentElement contentElement)
        {
            return ContentOperations.GetParent(contentElement)
                ?? (contentElement as FrameworkContentElement)?.Parent;
        }

        if (source is Visual or Visual3D)
        {
            return VisualTreeHelper.GetParent(source);
        }

        return LogicalTreeHelper.GetParent(source);
    }

    private void FilterReset_Click(object sender, RoutedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm?.ItemFilterReset();
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