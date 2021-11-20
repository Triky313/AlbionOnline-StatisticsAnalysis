using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootChestEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewLootChestEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewLootChestEvent value)
        {
            _trackingController?.DungeonController?.SetDungeonEventObjectInformationAsync(value.Id, value.UniqueName).ConfigureAwait(false);
            await Task.CompletedTask;
        }
    }
}