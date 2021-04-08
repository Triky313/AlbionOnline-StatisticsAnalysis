using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateMoneyEventHandler(TrackingController trackingController) : base(
            (int) EventCodes.UpdateMoney)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateMoneyEvent value)
        {
            await Task.CompletedTask;
        }
    }
}