using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class TestHandler
{
    public TestHandler()
    {
    }

    public async Task OnActionAsync(TestEvent testEvent)
    {
        await Task.CompletedTask;
    }
}