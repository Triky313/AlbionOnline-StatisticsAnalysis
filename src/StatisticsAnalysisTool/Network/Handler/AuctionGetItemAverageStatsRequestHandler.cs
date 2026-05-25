using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class AuctionGetItemAverageStatsRequestHandler : RequestPacketHandler<AuctionGetItemAverageStatsRequest>
{
    public AuctionGetItemAverageStatsRequestHandler(TrackingController trackingController) : base((int) OperationCodes.AuctionGetItemAverageStats)
    {
        _ = trackingController;
    }

    protected override Task OnActionAsync(AuctionGetItemAverageStatsRequest value)
    {
        var item = ItemController.GetItemByIndex(value.ItemIndex);
        if (item == null)
        {
            return Task.CompletedTask;
        }

        var marketLocation = Cluster.ClusterController.CurrentCluster.SourceClusterIndex?.GetMarketLocationByLocationNameOrId()
                             ?? Cluster.ClusterController.CurrentCluster.Index?.GetMarketLocationByLocationNameOrId()
                             ?? MarketLocation.Unknown;

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.CraftingBindings?.BlackMarket.CacheAverageStatsRequest(new BlackMarketAverageStatsRequestContext
        {
            RequestId = value.RequestId,
            ItemUniqueName = item.UniqueName,
            ItemIndex = value.ItemIndex,
            QualityLevel = value.QualityLevel,
            TimeRange = value.TimeRange,
            MarketLocation = marketLocation
        });

        return Task.CompletedTask;
    }
}