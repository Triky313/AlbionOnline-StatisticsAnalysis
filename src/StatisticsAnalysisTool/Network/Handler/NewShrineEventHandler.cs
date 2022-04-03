using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewShrineEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewShrineEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewShrineEvent value)
        {
            _trackingController.DungeonController?.SetDungeonEventObjectInformationAsync(value.Id, value.UniqueName).ConfigureAwait(false);
            await Task.CompletedTask;
        }
    }
}