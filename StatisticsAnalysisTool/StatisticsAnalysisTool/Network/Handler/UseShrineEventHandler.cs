using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UseShrineEventHandler : RequestPacketHandler<UseShrineOperation>
    {
        private readonly TrackingController _trackingController;

        public UseShrineEventHandler(TrackingController trackingController) : base((int) EventCodes.NewShrine)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(UseShrineOperation value)
        {
            await Task.CompletedTask;
        }
    }
}