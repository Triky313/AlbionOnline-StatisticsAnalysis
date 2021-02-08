using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        private readonly TrackingController _trackingController;
        public NewLootChestEventHandler(TrackingController trackingController) : base(363)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            _trackingController.SetDungeonChestInformation(value.Id, value.UniqueName);
            await Task.CompletedTask;
        }
    }
}