using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewShrineEventHandler(TrackingController trackingController) : EventPacketHandler<NewShrineEvent>((int) EventCodes.NewShrine)
{
    protected override async Task OnActionAsync(NewShrineEvent value)
    {
        await trackingController.DungeonController?.SetDungeonEventInformationAsync(value.Id, value.UniqueName)!;
        await Task.CompletedTask;
    }
}