using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class MoveRequestHandler : RequestPacketHandler<MoveOperation>
    {
        public MoveRequestHandler() : base(3) { }

        protected override async Task OnActionAsync(MoveOperation value)
        {
            Debug.Print($"Move request");
        }
    }
}