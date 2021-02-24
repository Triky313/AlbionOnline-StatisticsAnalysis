using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly FameCountUpTimer _fameCountUpTimer;

        public UpdateFameEventHandler(TrackingController trackingController, FameCountUpTimer fameCountUpTimer) : base((int) EventCodes.UpdateFame)
        {
            _trackingController = trackingController;
            _fameCountUpTimer = fameCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            _trackingController.AddNotification(SetPveFameNotification(value.TotalPlayerFame.DoubleValue, value.TotalGainedFame.DoubleValue, value.ZoneFame.DoubleValue, value.PremiumFame.DoubleValue, value.SatchelFame.DoubleValue, value.IsPremiumBonus));
            _fameCountUpTimer.Add(value.TotalGainedFame.DoubleValue);
            _trackingController.AddValueToDungeon(value.TotalGainedFame.DoubleValue, ValueType.Fame);

            _trackingController.SetTotalPlayerFame(value.TotalPlayerFame.DoubleValue);
            await Task.CompletedTask;
        }
        
        private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame, double satchelFame, bool isMobFame)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame, LanguageController.Translation("FAME"), FameTypeOperator.Pve, zoneFame, premiumFame, satchelFame, LanguageController.Translation("GAINED")),
            });
        }
    }
}