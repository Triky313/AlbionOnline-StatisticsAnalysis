using StatisticsAnalysisTool.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StatisticsAnalysisTool.Models.BindingModel;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for PlayerInformationControl.xaml
/// </summary>
public partial class PlayerInformationControl
{
    public PlayerInformationControl()
    {
        InitializeComponent();
    }

    private async void ListBoxUserSearch_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.AddedItems.OfType<PlayerInformationBindings.PlayerSearchStruct>().FirstOrDefault();
        if (string.IsNullOrWhiteSpace(selectedItem.Value?.Id)
            || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        await viewModel.PlayerInformationBindings.LoadPlayerDataAsync(selectedItem.Value);

        if (sender is ListBox listBox)
        {
            listBox.SelectedItem = null;
        }
    }

    private async void TextBoxPlayerSearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.PlayerInformationBindings.SelectedPlayerTabIndex = 0;
        await viewModel.PlayerInformationBindings.UpdateUsernameListBoxAsync(textBox.Text);
    }

    private async void SearchServer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded
            || PlayerSearchTextBox == null
            || DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        await viewModel.PlayerInformationBindings.UpdateUsernameListBoxAsync(PlayerSearchTextBox.Text);
    }
    private void CancelPlayerSearch_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PlayerInformationBindings.CancelSearch();
        }
    }
}
