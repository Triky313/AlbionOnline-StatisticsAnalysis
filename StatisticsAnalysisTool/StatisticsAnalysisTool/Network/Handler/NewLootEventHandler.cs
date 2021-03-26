using System.Diagnostics;
using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootEventHandler : EventPacketHandler<NewLootEvent>
    {
        public NewLootEventHandler() : base(88)
        {
        }

        protected override async Task OnActionAsync(NewLootEvent value)
        {
            Debug.Print("NewLoot");

            await Task.CompletedTask;
        }
    }
}