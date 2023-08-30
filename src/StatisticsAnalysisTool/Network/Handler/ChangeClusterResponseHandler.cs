using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ChangeClusterResponseHandler : ResponsePacketHandler<ChangeClusterResponse>
{
    private readonly TrackingController _trackingController;

    public ChangeClusterResponseHandler(TrackingController trackingController) : base((int) OperationCodes.ChangeCluster)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ChangeClusterResponse value)
    {
        _trackingController.ClusterController.ChangeClusterInformation(
            value.MapType, value.Guid, value.Index, value.IslandName, 
            value.WorldMapDataType, value.DungeonInformation, value.MainClusterIndex,
            value.MistsDungeonTier);
        _trackingController.EntityController.RemoveEntitiesByLastUpdate(2);
        _trackingController.DungeonController.ResetLocalPlayerDiscoveredLoot();

        await Task.CompletedTask;
    }
}