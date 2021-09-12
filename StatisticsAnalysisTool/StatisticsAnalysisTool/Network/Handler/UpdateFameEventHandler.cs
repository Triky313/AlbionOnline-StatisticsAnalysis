using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly CountUpTimer _countUpTimer;
        private readonly TrackingController _trackingController;

        public UpdateFameEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateFame)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController?.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            await _trackingController.AddNotificationAsync(SetPveFameNotification(value.TotalPlayerFame.DoubleValue, value.TotalGainedFame.DoubleValue,
                value.ZoneFame.DoubleValue, value.PremiumFame.DoubleValue, value.SatchelFame.DoubleValue, value.IsBonusFactorActive, value.BonusFactorInPercent));
            _countUpTimer.Add(ValueType.Fame, value.TotalGainedFame.DoubleValue);
            _trackingController.DungeonController?.AddValueToDungeon(value.TotalGainedFame.DoubleValue, ValueType.Fame);
        }

        private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame,
            double satchelFame, bool isBonusFactorActive, double bonusFactorInPercent)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame,
                    LanguageController.Translation("FAME"), PvpPveType.Pve, zoneFame, premiumFame, satchelFame, isBonusFactorActive, bonusFactorInPercent, 
                    LanguageController.Translation("GAINED"))
            }, NotificationType.Fame);
        }
    }
}