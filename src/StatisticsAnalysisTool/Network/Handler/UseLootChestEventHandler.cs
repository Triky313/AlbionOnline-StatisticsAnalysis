using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UseLootChestEventHandler
    {
        private readonly TrackingController _trackingController;

        public UseLootChestEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(UseLootChestResponse value)
        {
            Debug.Print($"Loot: {value.Loot.LooterName}");
            await Task.CompletedTask;
        }
    }
}