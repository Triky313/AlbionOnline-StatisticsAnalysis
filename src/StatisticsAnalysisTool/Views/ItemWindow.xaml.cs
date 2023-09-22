using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for ItemWindowNew.xaml
/// </summary>
public partial class ItemWindow
{
    private bool _isWindowMaximized;
    private readonly ItemWindowViewModel _itemWindowViewModel;

    public ItemWindow(Item item)
    {
        InitializeComponent(); 
        _itemWindowViewModel = new ItemWindowViewModel(this, item);
        DataContext = _itemWindowViewModel;
    }

    private void ItemWindow_OnClosing(object sender, CancelEventArgs e)
    {
        CraftingTabController.SaveInFile();

        var vm = (ItemWindowViewModel)DataContext;
        vm?.RemoveLocationFiltersEvents();
        vm?.RemoveTimerAsync();
        vm?.SaveSettings();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e?.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isWindowMaximized)
        {
            RestoreWindow();
        }
        else
        {
            MaximizeWindow();
        }
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (_isWindowMaximized)
            {
                RestoreWindow();
            }
            else
            {
                MaximizeWindow();
            }
        }
    }

    private void MaximizeWindow()
    {
        WindowState = WindowState.Maximized;
        _isWindowMaximized = true;
        var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        MaxHeight = screen.WorkingArea.Height;

        Visibility = Visibility.Hidden;
        Topmost = true;
        ResizeMode = ResizeMode.NoResize;
        Visibility = Visibility.Visible;
        MaximizedButton.Content = 2;
    }

    private void RestoreWindow()
    {
        WindowState = WindowState.Normal;
        _isWindowMaximized = false;
        Topmost = false;
        ResizeMode = ResizeMode.CanResize;
        MaximizedButton.Content = 1;
    }

    private void RefreshSpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (ItemWindowViewModel)DataContext;
        vm?.AutoUpdateSwitcher();
    }

    private void CraftingInfoPopup_MouseUp(object sender, MouseEventArgs e)
    {
        var vm = (ItemWindowViewModel)DataContext;
        vm?.CraftingTabBindings?.SetInfoPopupVisibility();
    }

    private void LabelNotes_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var vm = (ItemWindowViewModel)DataContext;
        _ = CraftingTabController.AddNoteAsync(vm?.Item.UniqueName, textBox.Text);
    }

    private ListSortDirection _lastDirection = ListSortDirection.Ascending;

    private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        GridViewColumnHeader column = sender as GridViewColumnHeader;
        string sortBy = column?.Tag.ToString();
        CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(_itemWindowViewModel.MainTabBindings.ItemPrices);

        view.SortDescriptions.Clear();
        if (sortBy != null)
        {
            _lastDirection = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            SortDescription sd = new SortDescription(sortBy, _lastDirection);
            view.SortDescriptions.Add(sd);
        }

        view.Refresh();
    }
}