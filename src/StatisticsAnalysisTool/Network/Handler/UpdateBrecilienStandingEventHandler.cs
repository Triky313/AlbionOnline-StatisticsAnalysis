using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateBrecilienStandingEventHandler : EventPacketHandler<UpdateBrecilienStandingEvent>
{
    private readonly TrackingController _trackingController;

    public UpdateBrecilienStandingEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateBrecilienStanding)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(UpdateBrecilienStandingEvent value)
    {
        _trackingController.DungeonController?.AddValueToDungeon(value.TotalPoints.DoubleValue, ValueType.BrecilianStanding);
        await Task.CompletedTask;
    }
}