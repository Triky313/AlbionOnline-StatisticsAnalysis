using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateMistCityStandingEventHandler : EventPacketHandler<UpdateMistCityStandingEvent>
{
    private readonly TrackingController _trackingController;

    public UpdateMistCityStandingEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateMistCityStanding)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(UpdateMistCityStandingEvent value)
    {
        _trackingController.DungeonController?.AddValueToDungeon(value.TotalPoints.DoubleValue, ValueType.BrecilianStanding);
        await Task.CompletedTask;
    }
}