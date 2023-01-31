using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringController
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public GatheringController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }


}