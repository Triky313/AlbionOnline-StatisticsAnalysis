using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Trade;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class CraftBuildingInfoEventHandler(TrackingController trackingController) : EventPacketHandler<CraftBuildingInfoEvent>((int) EventCodes.CraftBuildingInfo)
{
    protected override async Task OnActionAsync(CraftBuildingInfoEvent value)
    {
        trackingController.TradeController.AddCraftingBuildingInfo(new CraftingBuildingInfo { BuildingName = value.BuildingName, BuildingObjectId = value.BuildingObjectId, ObjectId = value.ObjectId });
        await Task.CompletedTask;
    }
}