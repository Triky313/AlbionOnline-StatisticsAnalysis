using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class GuildVaultInfoEventHandler
    {
        private readonly TrackingController _trackingController;

        public GuildVaultInfoEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(GuildVaultInfoEvent value)
        {

            await Task.CompletedTask;
        }
    }
}