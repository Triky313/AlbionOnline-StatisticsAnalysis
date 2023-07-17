using StatisticsAnalysisTool.ViewModels;
using System.Windows.Controls;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace StatisticsAnalysisTool.UserControls;
/// <summary>
/// Interaction logic for PartyPlanner.xaml
/// </summary>
public partial class PartyBuilderControl
{
    public PartyBuilderControl()
    {
        InitializeComponent();
    }

    private void MinimalItemPower_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && double.TryParse(textBox.Text, out double result))
        {
            var vm = (MainWindowViewModel) DataContext;
            vm?.PartyBuilderBindings?.UpdatePartyBuilderPlayerConditionsMinIP(result);
        }
    }

    private void MinimalBasicItemPower_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && double.TryParse(textBox.Text, out double result))
        {
            var vm = (MainWindowViewModel) DataContext;
            vm?.PartyBuilderBindings?.UpdatePartyBuilderPlayerConditionsMinBIP(result);
        }
    }

    private void MaximumItemPower_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && double.TryParse(textBox.Text, out double result))
        {
            var vm = (MainWindowViewModel) DataContext;
            vm?.PartyBuilderBindings?.UpdatePartyBuilderPlayerConditionsMaxIP(result);
        }
    }

    private void MaximumBasicItemPower_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && double.TryParse(textBox.Text, out double result))
        {
            var vm = (MainWindowViewModel) DataContext;
            vm?.PartyBuilderBindings?.UpdatePartyBuilderPlayerConditionsMaxBIP(result);
        }
    }
}
