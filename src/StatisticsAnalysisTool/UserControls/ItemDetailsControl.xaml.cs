using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ItemDetailsControl.xaml
/// </summary>
public partial class ItemDetailsControl
{
    private ListSortDirection _lastDirection = ListSortDirection.Ascending;

    public ItemDetailsControl()
    {
        InitializeComponent();
    }

    private async void Refresh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        if (DataContext is ItemDetailsViewModel viewModel)
        {
            await viewModel.RefreshManuallyAsync();
        }
    }

    private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not GridViewColumnHeader column
            || DataContext is not ItemDetailsViewModel viewModel
            || column.Tag?.ToString() is not { } sortBy)
        {
            return;
        }

        var view = (CollectionView) CollectionViewSource.GetDefaultView(viewModel.MainTabBindings.ItemPrices);
        view.SortDescriptions.Clear();

        _lastDirection = _lastDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        view.SortDescriptions.Add(new SortDescription(sortBy, _lastDirection));
        view.Refresh();
    }
}
