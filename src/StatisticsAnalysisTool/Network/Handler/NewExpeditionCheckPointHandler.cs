using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewExpeditionCheckPointHandler : EventPacketHandler<NewExpeditionCheckPointEvent>
{
    private readonly TrackingController _trackingController;

    public NewExpeditionCheckPointHandler(TrackingController trackingController) : base((int) EventCodes.NewExpeditionCheckPoint)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewExpeditionCheckPointEvent value)
    {
        await _trackingController.DungeonController.UpdateCheckPointAsync(new CheckPoint() { Id = value.ObjectId, Status = value.Status });
    }
}