using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.ViewModels;
using System.Windows.Controls;

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

    private void CraftingLocationSearch_OnGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.CraftingBindings.OpenCraftingLocationSearch();
    }
}
