using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UseLootChestEventHandler : ResponsePacketHandler<UseLootChestResponse>
    {
        private readonly TrackingController _trackingController;

        public UseLootChestEventHandler(TrackingController trackingController) : base((int) OperationCodes.UseLootChest)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(UseLootChestResponse value)
        {
            Debug.Print($"Loot: {value.Loot.LooterName}");
            await Task.CompletedTask;
        }
    }
}