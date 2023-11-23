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
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_RESOURCES"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_RESOURCES"));
        var dialogResult = dialog.ShowDialog();

        var vm = (MainWindowViewModelOld) DataContext;

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
        if ((MainWindowViewModelOld) DataContext is not { GatheringBindings.GatheredCollection: { } } mainWindowViewModel)
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
