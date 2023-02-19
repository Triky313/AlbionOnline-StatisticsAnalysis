using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for ItemWindowNew.xaml
/// </summary>
public partial class ItemWindow
{
    public ItemWindow(Item item)
    {
        InitializeComponent();
        var itemWindowViewModel = new ItemWindowViewModel(this, item);
        DataContext = itemWindowViewModel;
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
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        switch (e?.ClickCount)
        {
            case 2 when WindowState == WindowState.Normal:
                WindowState = WindowState.Maximized;
                return;
            case 2 when WindowState == WindowState.Maximized:
                WindowState = WindowState.Normal;
                break;
        }
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
}