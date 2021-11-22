using System.Diagnostics;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootEventHandler
    {
        public NewLootEventHandler()
        {
        }

        public async Task OnActionAsync(NewLootEvent value)
        {
            Debug.Print("NewLoot");

            await Task.CompletedTask;
        }
    }
}