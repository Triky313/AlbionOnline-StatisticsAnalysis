using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StatisticsAnalysisTool.UserControls;

public partial class CraftingControl
{
    public CraftingControl()
    {
        InitializeComponent();
    }

    private void SavedCraftings_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        if (sender is not ListView listView)
        {
            return;
        }

        if (listView.SelectedItem is not SavedCrafting savedCrafting)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.LoadSelectedCommand.Execute(savedCrafting);
    }

    private void ListBoxItemSearch_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        if (sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not CraftingItemSearchResult searchResult)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.SelectItemSearchResult(searchResult);
        listBox.SelectedItem = null;
    }

    private void ListBoxCraftingLocation_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        if (sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not CraftingLocationOption location)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.SelectCraftingLocation(location);
        listBox.SelectedItem = null;
    }

    private void ListBoxSellPrice_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        if (sender is not ListBox listBox)
        {
            return;
        }

        if (listBox.SelectedItem is not CraftingSellPriceOption sellPriceOption)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.SelectSellPriceOption(sellPriceOption);
        listBox.SelectedItem = null;
    }

    private void CraftingLocationSearch_OnGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.OpenCraftingLocationSearch();
    }

    private void OutputUnitPrice_OnGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.OpenSellPriceOptions();
    }

    private void OutputUnitPrice_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var newFocus = e.NewFocus as DependencyObject;

        if (IsElementOrChildOf(newFocus, OutputUnitPriceTextBox)
            || IsElementOrChildOf(newFocus, SellPriceOptionsListBox))
        {
            return;
        }

        CloseSellPriceOptions();
    }

    private void OutputUnitPrice_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.OpenSellPriceOptions();
    }

    private void CraftingControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsElementOrChildOf(e.OriginalSource as DependencyObject, OutputUnitPriceTextBox))
        {
            return;
        }

        CloseSellPriceOptions();
    }

    private void CloseSellPriceOptions()
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.CloseSellPriceOptions();
    }

    private static bool IsElementOrChildOf(DependencyObject source, DependencyObject parent)
    {
        while (source != null)
        {
            if (ReferenceEquals(source, parent))
            {
                return true;
            }

            source = GetParent(source);
        }

        return false;
    }

    private static DependencyObject GetParent(DependencyObject source)
    {
        if (source is Visual
            || source is System.Windows.Media.Media3D.Visual3D)
        {
            return VisualTreeHelper.GetParent(source);
        }

        return LogicalTreeHelper.GetParent(source);
    }
}
