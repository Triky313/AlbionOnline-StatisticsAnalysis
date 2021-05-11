using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    /// <summary>
    ///     Triggered when silver is picked up. Each party member gets their own event.
    /// </summary>
    public class SiegeCampClaimStartEventHandler : EventPacketHandler<SiegeCampClaimStartEvent>
    {
        private readonly TrackingController _trackingController;

        public SiegeCampClaimStartEventHandler(TrackingController trackingController) : base((int) EventCodes.SiegeCampClaimStart)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(SiegeCampClaimStartEvent value)
        {
            await Task.CompletedTask;
        }
    }
}