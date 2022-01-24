using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class MightFavorPointsEventHandler
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public MightFavorPointsEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController?.CountUpTimer;
        }

        public async Task OnActionAsync(MightFavorPointsEvent value)
        {
            _trackingController.StatisticController?.AddValue(ValueType.Might, value.Might.DoubleValue);
            _trackingController.StatisticController?.AddValue(ValueType.Favor, value.Favor.DoubleValue);
            _countUpTimer.Add(ValueType.Might, value.Might.DoubleValue);
            _countUpTimer.Add(ValueType.Favor, value.Favor.DoubleValue);

            _trackingController.DungeonController?.AddValueToDungeon(value.Might.DoubleValue, ValueType.Might);
            _trackingController.DungeonController?.AddValueToDungeon(value.Favor.DoubleValue, ValueType.Favor);

            await Task.CompletedTask;
        }
    }
}