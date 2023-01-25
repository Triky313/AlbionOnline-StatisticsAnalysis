using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Trade;

public class TradeController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public TradeController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }
}