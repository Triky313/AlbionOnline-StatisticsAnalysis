using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
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