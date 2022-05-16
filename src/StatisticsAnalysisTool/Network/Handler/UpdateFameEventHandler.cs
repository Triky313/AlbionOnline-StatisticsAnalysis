using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler
    {
        private readonly CountUpTimer _countUpTimer;
        private readonly TrackingController _trackingController;

        public UpdateFameEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController?.CountUpTimer;
        }

        public async Task OnActionAsync(UpdateFameEvent value)
        {
            await _trackingController.AddNotificationAsync(SetPveFameNotification(value.TotalPlayerFame.DoubleValue, value.TotalGainedFame,
                value.ZoneFame.DoubleValue, value.PremiumFame, value.SatchelFame.DoubleValue, value.IsBonusFactorActive, value.BonusFactorInPercent));
            _countUpTimer.Add(ValueType.Fame, value.TotalGainedFame);
            _trackingController.DungeonController?.AddValueToDungeon(value.TotalGainedFame, ValueType.Fame);
            _trackingController.StatisticController?.AddValue(ValueType.Fame, value.TotalGainedFame);
        }

        private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame,
            double satchelFame, bool isBonusFactorActive, double bonusFactorInPercent)
        {
            return new TrackingNotification(DateTime.Now, new FameNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame,
                LanguageController.Translation("FAME"), PvpPveType.Pve, zoneFame, premiumFame, satchelFame, isBonusFactorActive, bonusFactorInPercent,
                LanguageController.Translation("GAINED")), NotificationType.Fame);
        }
    }
}