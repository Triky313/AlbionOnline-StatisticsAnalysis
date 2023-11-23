using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingFinishedEventHandler : EventPacketHandler<ActionOnBuildingFinishedEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public ActionOnBuildingFinishedEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.ActionOnBuildingFinished)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(ActionOnBuildingFinishedEvent value)
    {
        if (value is { UserObjectId: { } userObjectIdForRepair, ActionType: ActionOnBuildingType.Repair })
        {
            _gameEventWrapper.TrackingController.RepairFinished(userObjectIdForRepair, value.BuildingObjectId);
        }

        if (value is { UserObjectId: { } userObjectIdForBuy, ActionType: ActionOnBuildingType.BuyAndCrafting })
        {
            await _gameEventWrapper.TradeController.TradeFinishedAsync(userObjectIdForBuy, value.BuildingObjectId);
        }
    }
}