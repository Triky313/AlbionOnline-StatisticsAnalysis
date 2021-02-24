using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler3 : RequestPacketHandler<TestEvent3>
    {
        public TestHandler3() : base((int) OperationCodes.ChangeCluster) { }

        protected override async Task OnActionAsync(TestEvent3 value)
        {
            await Task.CompletedTask;
        }
    }
}