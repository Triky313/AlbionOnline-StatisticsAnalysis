using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingStartRequestHandler : RequestPacketHandler<ActionOnBuildingStartRequest>
{
    private readonly TrackingController _trackingController;

    public ActionOnBuildingStartRequestHandler(TrackingController trackingController) : base((int) OperationCodes.ActionOnBuildingStart)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ActionOnBuildingStartRequest value)
    {
        _trackingController.SetUpcomingRepair(value.BuildingObjectId, value.Costs);
        if (value.ActionType == Enumerations.ActionOnBuildingType.RerollQuality)
        {
            _trackingController.SetUpcomingQualityReroll(
                value.ItemObjectIds,
                value.ItemQuantities,
                value.ItemQualities,
                value.Costs);
        }
        else if (value.ActionType == Enumerations.ActionOnBuildingType.AwakenedWeapon)
        {
            _trackingController.SetUpcomingAwakenedWeaponAction(
                value.BuildingObjectId,
                value.Ticks,
                value.AwakenedWeaponSilverCosts);
        }

        if (value.ActionType == Enumerations.ActionOnBuildingType.BuyAndCrafting)
        {
            var isMerchantPurchase = value.ItemIndex > 0
                                     && value.ItemObjectIds.Count == 0
                                     && value.ItemQuantities.Count == 0;
            _trackingController.TradeController.SetUpcomingTrade(
                value.BuildingObjectId,
                value.Ticks,
                value.Costs,
                value.Quantity,
                value.ItemIndex,
                isMerchantPurchase);
        }

        await Task.CompletedTask;
    }
}