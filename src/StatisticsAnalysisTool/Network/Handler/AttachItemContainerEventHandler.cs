using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class AttachItemContainerEventHandler
    {
        private readonly TrackingController _trackingController;

        public AttachItemContainerEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(AttachItemContainerEvent value)
        {
            if (_trackingController.IsTrackingAllowedByMainCharacter())
            {
                _trackingController.VaultController.AddContainer(value.ItemContainerObject);
            }

            await Task.CompletedTask;
        }
    }
}