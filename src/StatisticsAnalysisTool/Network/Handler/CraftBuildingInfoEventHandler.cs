using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Trade;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class CraftBuildingInfoEventHandler : EventPacketHandler<CraftBuildingInfoEvent>
{
    private readonly ITradeController _tradeController;

    public CraftBuildingInfoEventHandler(ITradeController tradeController) : base((int) EventCodes.CraftBuildingInfo)
    {
        _tradeController = tradeController;
    }

    protected override async Task OnActionAsync(CraftBuildingInfoEvent value)
    {
        _tradeController.AddCraftingBuildingInfo(new CraftingBuildingInfo { BuildingName = value.BuildingName, BuildingObjectId = value.BuildingObjectId, ObjectId = value.ObjectId });
        await Task.CompletedTask;
    }
}