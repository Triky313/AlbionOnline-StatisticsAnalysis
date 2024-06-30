using System.Linq;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Windows;
using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.UserControls;
/// <summary>
/// Interaction logic for GatheringControl.xaml
/// </summary>
public partial class GatheringControl
{
    private bool _isSelectAllActive;

    public GatheringControl()
    {
        InitializeComponent();
    }

    public void DeleteSelectedResources()
    {
        var dialog = new DialogWindow(LocalizationController.Translation("DELETE_SELECTED_RESOURCES"), LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_RESOURCES"));
        var dialogResult = dialog.ShowDialog();

        var vm = (MainWindowViewModel) DataContext;

        if (dialogResult is true)
        {
            var selectedResourceGuids = vm?.GatheringBindings?.GatheredCollection?.Where(x => x?.IsSelectedForDeletion ?? false).Select(x => x.Guid);
            vm?.GatheringBindings?.RemoveResourcesByIdsAsync(selectedResourceGuids);
        }
    }

    private void BtnDeleteSelectedGathered_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedResources();
    }

    private void BtnSelectSwitchAllGathered_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel) DataContext is not { GatheringBindings.GatheredCollection: { } } mainWindowViewModel)
        {
            return;
        }

        foreach (var gathered in mainWindowViewModel.GatheringBindings.GatheredCollection)
        {
            gathered.IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }
}
