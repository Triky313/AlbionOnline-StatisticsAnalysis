using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ChangeClusterResponseHandler : ResponsePacketHandler<ChangeClusterResponse>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public ChangeClusterResponseHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.ChangeCluster)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(ChangeClusterResponse value)
    {
        _gameEventWrapper.ClusterController.ChangeClusterInformation(
            value.MapType, value.Guid, value.Index, value.IslandName, 
            value.WorldMapDataType, value.DungeonInformation, value.MainClusterIndex,
            value.MistsDungeonTier);
        _gameEventWrapper.EntityController.RemoveEntitiesByLastUpdate(2);
        _gameEventWrapper.DungeonController.ResetLocalPlayerDiscoveredLoot();

        await Task.CompletedTask;
    }
}