using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class DiedEventHandler : EventPacketHandler<DiedEvent>
    {
        private readonly TrackingController _trackingController;

        public DiedEventHandler(TrackingController trackingController) : base(153)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(DiedEvent value)
        {
            _trackingController.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
            await Task.CompletedTask;
        }
    }
}