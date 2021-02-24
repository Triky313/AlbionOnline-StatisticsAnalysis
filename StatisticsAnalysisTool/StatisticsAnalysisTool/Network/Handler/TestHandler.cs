using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler : ResponsePacketHandler<TestEvent>
    {
        public TestHandler() : base((int) OperationCodes.ChangeCluster) { }

        protected override async Task OnActionAsync(TestEvent testEvent)
        {
            await Task.CompletedTask;
        }
    }
}