using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    /// <summary>
    ///     Triggered when silver is picked up. Each party member gets their own event.
    /// </summary>
    public class SiegeCampClaimStartEventHandler
    {
        private readonly TrackingController _trackingController;

        public SiegeCampClaimStartEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(SiegeCampClaimStartEvent value)
        {
            await Task.CompletedTask;
        }
    }
}