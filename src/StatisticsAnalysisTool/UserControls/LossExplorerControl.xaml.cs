using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

public partial class LossExplorerControl
{
    public LossExplorerControl()
    {
        InitializeComponent();
    }

    private async void LossExplorerControl_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not true || DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        var lossExplorer = mainWindowViewModel.CraftingBindings.LossExplorer;
        if (lossExplorer != null)
        {
            await lossExplorer.LoadAsync();
        }
    }

    private void LossExplorerFilterReset_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel mainWindowViewModel)
        {
            mainWindowViewModel.CraftingBindings.LossExplorer?.ResetFilters();
        }
    }
}