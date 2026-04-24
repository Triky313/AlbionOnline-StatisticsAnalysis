using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Cluster;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewRandomDungeonExitEventHandler(TrackingController trackingController) : EventPacketHandler<NewRandomDungeonExitEvent>((int) EventCodes.NewRandomDungeonExit)
{
    protected override Task OnActionAsync(NewRandomDungeonExitEvent value)
    {
        trackingController.DungeonController.AddRandomDungeonExit(value.ToRandomDungeonExitInfo(ClusterController.CurrentCluster.Index));
        return Task.CompletedTask;
    }
}