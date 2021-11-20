using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler2
    {
        public TestHandler2()
        {
        }

        public async Task OnActionAsync(TestEvent2 value)
        {
            await Task.CompletedTask;
        }
    }
}