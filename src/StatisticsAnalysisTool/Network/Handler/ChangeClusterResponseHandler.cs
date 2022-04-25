using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ChangeClusterResponseHandler
    {
        private readonly TrackingController _trackingController;

        public ChangeClusterResponseHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(ChangeClusterResponse value)
        {
            _trackingController.ClusterController.ChangeClusterInformation(value.MapType, value.Guid, value.Index, value.IslandName, value.WorldMapDataType, value.DungeonInformation, value.MainClusterIndex);

            _trackingController.EntityController.RemoveAllEntities();
            _trackingController.LootController.ResetViewedLootLists();

            await Task.CompletedTask;
        }
    }
}