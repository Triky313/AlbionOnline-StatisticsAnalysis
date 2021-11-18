using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateSilverEventHandler : EventPacketHandler<UpdateSilverEvent>
    {
        private readonly CountUpTimer _countUpTimer;

        public UpdateSilverEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateSilver)
        {
            _countUpTimer = trackingController?.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateSilverEvent value)
        {
            await Task.CompletedTask;
        }
    }
}