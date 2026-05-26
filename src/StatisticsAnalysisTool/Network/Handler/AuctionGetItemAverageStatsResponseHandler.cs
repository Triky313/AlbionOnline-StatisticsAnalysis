using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
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
        if (!SettingsController.CurrentSettings.Bm)
        {
            return;
        }

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        var blackMarket = mainWindowViewModel.CraftingBindings.BlackMarket;
        if (blackMarket != null)
        {
            await blackMarket.RecordAverageStatsResponseAsync(value.RequestId, value.Points);
        }
    }
}