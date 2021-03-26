using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly SilverCountUpTimer _silverCountUpTimer;

        public UpdateMoneyEventHandler(TrackingController trackingController, SilverCountUpTimer silverCountUpTimer) : base((int) EventCodes.UpdateMoney)
        {
            _trackingController = trackingController;
            _silverCountUpTimer = silverCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateMoneyEvent value)
        {
            _silverCountUpTimer.Add(value.CurrentPlayerSilver);

            _trackingController.SetTotalPlayerSilver(value.CurrentPlayerSilver);
            _trackingController.DungeonController?.AddValueToDungeon(value.CurrentPlayerSilver, ValueType.Silver);
            await Task.CompletedTask;
        }
    }
}