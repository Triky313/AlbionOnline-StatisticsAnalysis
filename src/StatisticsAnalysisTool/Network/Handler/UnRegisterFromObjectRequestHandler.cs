using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UnRegisterFromObjectRequestHandler
    {
        private readonly TrackingController _trackingController;

        public UnRegisterFromObjectRequestHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(UnRegisterFromObjectRequest value)
        {
            _trackingController.UnregisterBuilding(value.BuildingObjectId);
            await Task.CompletedTask;
        }
    }
}