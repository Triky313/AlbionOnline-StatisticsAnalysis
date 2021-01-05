using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class MoveEventHandler : EventPacketHandler<MoveEvent>
    {
        public MoveEventHandler() : base(3) { }

        protected override async Task OnActionAsync(MoveEvent value)
        {
            Debug.Print($"Id: {value.Id} x: {value.Position[0]} y: {value.Position[1]}");
        }
    }
}