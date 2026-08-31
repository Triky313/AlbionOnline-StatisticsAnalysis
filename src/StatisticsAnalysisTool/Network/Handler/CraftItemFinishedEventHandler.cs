using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class CraftItemFinishedEventHandler(TrackingController trackingController) : EventPacketHandler<CraftItemFinishedEvent>((int) EventCodes.CraftItemFinished)
{
    protected override async Task OnActionAsync(CraftItemFinishedEvent value)
    {
        if (value.UserObjectId is { } userObjectId)
        {
            trackingController.TradeController.ConfirmUpcomingCraftingTrade(userObjectId, value.BuildingObjectId);
        }

        await Task.CompletedTask;
    }
}