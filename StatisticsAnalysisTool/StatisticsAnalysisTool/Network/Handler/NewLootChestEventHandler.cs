using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        public NewLootChestEventHandler() : base(EventCodes.NewLootChest) { }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            Debug.Print($"NewSilverObject");

            await Task.CompletedTask;
        }
    }
}