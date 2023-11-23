using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingStartRequestHandler : RequestPacketHandler<ActionOnBuildingStartRequest>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public ActionOnBuildingStartRequestHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.ActionOnBuildingStart)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(ActionOnBuildingStartRequest value)
    {
        _gameEventWrapper.TrackingController.SetUpcomingRepair(value.BuildingObjectId, value.Costs);
        _gameEventWrapper.TradeController.SetUpcomingTrade(value.BuildingObjectId, value.Ticks, value.Costs, value.Quantity, value.ItemIndex);
        await Task.CompletedTask;
    }
}