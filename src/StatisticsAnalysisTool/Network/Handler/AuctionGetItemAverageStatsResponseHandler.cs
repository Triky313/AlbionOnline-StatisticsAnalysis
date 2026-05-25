using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class AuctionGetItemAverageStatsResponseHandler : ResponsePacketHandler<AuctionGetItemAverageStatsResponse>
{
    public AuctionGetItemAverageStatsResponseHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionGetItemAverageStats)
    {
        _ = trackingController;
    }

    protected override async Task OnActionAsync(AuctionGetItemAverageStatsResponse value)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        await mainWindowViewModel.CraftingBindings.BlackMarket.RecordAverageStatsResponseAsync(value.RequestId, value.Points);
    }
}