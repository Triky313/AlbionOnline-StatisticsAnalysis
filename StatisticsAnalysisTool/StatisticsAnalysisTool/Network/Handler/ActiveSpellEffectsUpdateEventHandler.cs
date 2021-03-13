using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ActiveSpellEffectsUpdateEventHandler : EventPacketHandler<ActiveSpellEffectsUpdateEvent>
    {
        private readonly TrackingController _trackingController;
        public ActiveSpellEffectsUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.ActiveSpellEffectsUpdate)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(ActiveSpellEffectsUpdateEvent value)
        {

            await Task.CompletedTask;
        }
    }
}