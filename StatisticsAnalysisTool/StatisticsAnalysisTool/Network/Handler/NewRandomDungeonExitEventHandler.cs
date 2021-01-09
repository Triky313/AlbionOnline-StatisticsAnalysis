using Albion.Network;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewRandomDungeonExitEventHandler : EventPacketHandler<NewRandomDungeonExitEvent>
    {
        public NewRandomDungeonExitEventHandler() : base(364) { }

        protected override async Task OnActionAsync(NewRandomDungeonExitEvent value)
        {
            await Task.CompletedTask;
        }
    }
}