using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        public NewLootChestEventHandler() : base(34) { }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            Debug.Print($"NewSilverObject");
        }
    }
}