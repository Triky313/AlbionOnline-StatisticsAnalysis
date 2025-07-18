using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewHellDungeonRoomShrineObjectEventHandler(TrackingController trackingController) : EventPacketHandler<NewHellDungeonRoomShrineObjectEvent>((int) EventCodes.NewHellDungeonRoomShrineObject)
{
    protected override async Task OnActionAsync(NewHellDungeonRoomShrineObjectEvent value)
    {
        await trackingController.DungeonController?.SetDungeonEventInformationAsync(value.Id, value.ObjectName)!;
        await Task.CompletedTask;
    }
}