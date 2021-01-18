using Albion.Network;
using StatisticsAnalysisTool.Common;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly SilverCountUpTimer _silverCountUpTimer;

        public UpdateMoneyEventHandler(TrackingController trackingController, SilverCountUpTimer silverCountUpTimer) : base(EventCodes.UpdateMoney)
        {
            _trackingController = trackingController;
            _silverCountUpTimer = silverCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateMoneyEvent value)
        {
            _silverCountUpTimer.Add(value.CurrentPlayerSilver);

            _trackingController.SetTotalPlayerSilver(Formatting.ToStringShort(value.CurrentPlayerSilver));
            await Task.CompletedTask;
        }
    }
}