using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Requests;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UseShrineRequestHandler
    {
        private readonly TrackingController _trackingController;

        public UseShrineRequestHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(UseShrineRequest value)
        {
            await Task.CompletedTask;
        }
    }
}