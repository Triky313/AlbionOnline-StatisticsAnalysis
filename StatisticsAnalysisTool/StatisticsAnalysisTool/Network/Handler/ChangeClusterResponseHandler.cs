using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ChangeClusterResponseHandler : ResponsePacketHandler<ChangeClusterResponse>
    {
        private readonly TrackingController _trackingController;

        public ChangeClusterResponseHandler(TrackingController trackingController) : base((int) OperationCodes.ChangeCluster)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(ChangeClusterResponse value)
        {
            _trackingController.AddDebugNotification(HandlerType.Operation, (int)OperationCodes.ChangeCluster, JsonConvert.SerializeObject(value));

            _trackingController.EntityController.RemoveAllEntities();
            await Task.CompletedTask;
        }
    }
}