using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingFinishedEventHandler : EventPacketHandler<ActionOnBuildingFinishedEvent>
{
    private readonly TrackingController _trackingController;

    public ActionOnBuildingFinishedEventHandler(TrackingController trackingController) : base((int) EventCodes.ActionOnBuildingFinished)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ActionOnBuildingFinishedEvent value)
    {
        if (value is { UserObjectId: { } userObjectIdForRepair, ActionType: ActionOnBuildingType.Repair })
        {
            _trackingController.RepairFinished(userObjectIdForRepair, value.BuildingObjectId);
        }

        if (value is { UserObjectId: { } userObjectIdForBuy, ActionType: ActionOnBuildingType.Buy })
        {
            await _trackingController.TradeController.TradeFinishedAsync(userObjectIdForBuy, value.BuildingObjectId);
        }
    }
}