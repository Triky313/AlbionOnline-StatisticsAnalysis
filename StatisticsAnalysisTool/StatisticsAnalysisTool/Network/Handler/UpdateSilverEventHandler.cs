using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateSilverEventHandler
    {
        private readonly CountUpTimer _countUpTimer;

        public UpdateSilverEventHandler(TrackingController trackingController)
        {
            _countUpTimer = trackingController?.CountUpTimer;
        }

        public async Task OnActionAsync(UpdateSilverEvent value)
        {
            await Task.CompletedTask;
        }
    }
}