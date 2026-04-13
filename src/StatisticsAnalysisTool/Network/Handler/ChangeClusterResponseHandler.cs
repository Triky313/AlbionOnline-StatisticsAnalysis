using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ChangeClusterResponseHandler(TrackingController trackingController) : ResponsePacketHandler<ChangeClusterResponse>((int) OperationCodes.ChangeCluster)
{
    protected override async Task OnActionAsync(ChangeClusterResponse value)
    {
        trackingController.ClusterController.ChangeClusterInformation(
            value.MapType, value.Guid, value.Index, value.IslandName,
            value.WorldMapDataType, value.DungeonInformation, value.MainClusterIndex,
            value.MistsDungeonTier);
        trackingController.EntityController.RemoveEntitiesByLastUpdate(2);
        trackingController.DungeonController.ResetLocalPlayerDiscoveredLoot();

        await Task.CompletedTask;
    }
}