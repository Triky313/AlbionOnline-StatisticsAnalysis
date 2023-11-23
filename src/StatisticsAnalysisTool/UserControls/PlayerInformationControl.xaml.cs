using StatisticsAnalysisTool.ViewModels;
using System.Linq;
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
        var selectedItem = e?.AddedItems?.OfType<PlayerInformationBindings.PlayerSearchStruct>().FirstOrDefault();
        if (selectedItem?.Value?.Name == null)
        {
            return;
        }

        var vm = (MainWindowViewModelOld)DataContext;
        await vm?.PlayerInformationBindings?.LoadPlayerDataAsync(selectedItem.Value.Value?.Name)!;
    }

    private async void TextBoxPlayerSearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var vm = (MainWindowViewModelOld)DataContext;
            await vm?.PlayerInformationBindings?.UpdateUsernameListBoxAsync(textBox.Text)!;
        }
    }
}