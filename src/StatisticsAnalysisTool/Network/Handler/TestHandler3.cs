using System.Threading.Tasks;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class TestHandler3
{
    public TestHandler3()
    {
    }

    public async Task OnActionAsync(TestEvent3 value)
    {
        await Task.CompletedTask;
    }
}