using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ActionOnBuildingFinishedEventHandler
{
    private readonly TrackingController _trackingController;

    public ActionOnBuildingFinishedEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(ActionOnBuildingFinishedEvent value)
    {
        if (value is {UserObjectId: { } userObjectId, ActionType: ActionOnBuildingType.Repair})
        {
            _trackingController.RepairFinished(userObjectId, value.BuildingObjectId);
        }

        await Task.CompletedTask;
    }
}