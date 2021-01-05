using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
    {
        public NewCharacterEventHandler() : base(25) { }

        protected override async Task OnActionAsync(NewCharacterEvent value)
        {
            Debug.Print($"New ch Name: {value.Name}");
        }
    }
}