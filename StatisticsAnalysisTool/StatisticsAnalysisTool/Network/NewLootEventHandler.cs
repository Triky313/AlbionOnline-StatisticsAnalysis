using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class NewLootEventHandler : EventPacketHandler<NewLootEvent>
    {
        public NewLootEventHandler() : base(88) { }

        protected override async Task OnActionAsync(NewLootEvent value)
        {
            Debug.Print($"NewLoot");
        }
    }
}