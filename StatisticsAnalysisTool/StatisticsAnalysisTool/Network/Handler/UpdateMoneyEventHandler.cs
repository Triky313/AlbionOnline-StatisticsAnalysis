using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
    {
        private readonly CountUpTimer _countUpTimer;

        public UpdateMoneyEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateMoney)
        {
            _countUpTimer = trackingController.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateMoneyEvent value)
        {
            await Task.CompletedTask;
        }
    }
}