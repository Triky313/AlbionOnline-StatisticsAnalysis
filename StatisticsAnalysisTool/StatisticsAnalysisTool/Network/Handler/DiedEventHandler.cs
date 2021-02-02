using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class DiedEventHandler : EventPacketHandler<DiedEvent>
    {
        private readonly TrackingController _trackingController;

        public DiedEventHandler(TrackingController trackingController) : base(EventCodes.UpdateFame)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(DiedEvent value)
        {
            _trackingController.SetDiedIfInDungeon();
            await Task.CompletedTask;
        }
    }
}