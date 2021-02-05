using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        public NewLootChestEventHandler(TrackingController trackingController) : base(363) { }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            await Task.CompletedTask;
        }
    }
}