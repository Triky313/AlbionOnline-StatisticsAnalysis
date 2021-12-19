using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class MightFavorPointsEventHandler
    {
        private readonly TrackingController _trackingController;

        public MightFavorPointsEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(MightFavorPointsEvent value)
        {
            _trackingController.StatisticController?.AddValue(ValueType.Might, value.Might.DoubleValue);
            _trackingController.StatisticController?.AddValue(ValueType.Favor, value.Favor.DoubleValue);

            await Task.CompletedTask;
        }
    }
}