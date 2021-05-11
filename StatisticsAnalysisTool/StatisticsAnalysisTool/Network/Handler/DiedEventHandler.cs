using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class DiedEventHandler : EventPacketHandler<DiedEvent>
    {
        private readonly TrackingController _trackingController;

        public DiedEventHandler(TrackingController trackingController) : base((int)EventCodes.Died)
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