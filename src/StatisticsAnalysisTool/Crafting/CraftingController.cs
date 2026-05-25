using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingController
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public CraftingController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _ = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public async Task SaveInFileAsync()
    {
        if (_mainWindowViewModel?.CraftingBindings == null)
        {
            return;
        }

        await _mainWindowViewModel.CraftingBindings.SaveInFileAsync().ConfigureAwait(false);
    }
}