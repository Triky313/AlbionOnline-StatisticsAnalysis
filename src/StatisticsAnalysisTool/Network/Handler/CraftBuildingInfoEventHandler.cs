using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Trade;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class CraftBuildingInfoEventHandler : EventPacketHandler<CraftBuildingInfoEvent>
{
    private readonly TrackingController _trackingController;

    public CraftBuildingInfoEventHandler(TrackingController trackingController) : base((int) EventCodes.CraftBuildingInfo)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(CraftBuildingInfoEvent value)
    {
        _trackingController.TradeController.AddCraftingBuildingInfo(new CraftingBuildingInfo { BuildingName = value.BuildingName, BuildingObjectId = value.BuildingObjectId, ObjectId = value.ObjectId });
        await Task.CompletedTask;
    }
}