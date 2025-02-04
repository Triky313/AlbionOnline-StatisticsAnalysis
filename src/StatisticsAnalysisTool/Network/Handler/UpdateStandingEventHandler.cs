using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateStandingEventHandler : EventPacketHandler<UpdateStandingEvent>
{
    private readonly TrackingController _trackingController;

    public UpdateStandingEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateStanding)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(UpdateStandingEvent value)
    {
        _trackingController.DungeonController?.AddValueToDungeon(value.TotalPoints.DoubleValue, ValueType.BrecilianStanding);
        await Task.CompletedTask;
    }
}