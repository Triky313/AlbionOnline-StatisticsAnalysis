using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler : ResponsePacketHandler<TestEvent>
    {
        public TestHandler() : base((int) OperationCodes.Join)
        {
        }

        protected override async Task OnActionAsync(TestEvent testEvent)
        {
            await Task.CompletedTask;
        }
    }
}