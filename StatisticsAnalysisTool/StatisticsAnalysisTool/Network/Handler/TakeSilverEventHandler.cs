using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        private readonly TrackingController _trackingController;

        public TakeSilverEventHandler(TrackingController trackingController) : base((int) EventCodes.TakeSilver)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            var localEntity = _trackingController.EntityController.GetLocalEntity()?.Value;
            if (localEntity?.ObjectId == value.ObjectId)
            {
                _trackingController.CountUpTimer.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.YieldAfterTax.DoubleValue, ValueType.Silver);
            }

            await Task.CompletedTask;
        }
    }
}