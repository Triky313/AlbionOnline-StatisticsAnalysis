using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ReSpecBoostRequestHandler
    {
        private readonly TrackingController _trackingController;

        public ReSpecBoostRequestHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(ReSpecBoostRequest value)
        {
            _trackingController.EntityController.LocalUserData.IsReSpecActive = value.IsReSpecBoostActive;
            await Task.CompletedTask;
        }
    }
}