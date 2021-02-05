using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class LootChestOpenedEventHandler : EventPacketHandler<LootChestOpenedEvent>
    {
        public LootChestOpenedEventHandler(TrackingController trackingController) : base(365) { }

        protected override async Task OnActionAsync(LootChestOpenedEvent value)
        {
            await Task.CompletedTask;
        }
    }
}