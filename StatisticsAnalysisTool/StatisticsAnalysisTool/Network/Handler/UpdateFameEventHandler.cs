using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly FameCountUpTimer _fameCountUpTimer;

        public UpdateFameEventHandler(TrackingController trackingController, FameCountUpTimer fameCountUpTimer) : base(EventCodes.UpdateFame)
        {
            _trackingController = trackingController;
            _fameCountUpTimer = fameCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            _trackingController.AddNotification(SetPveFameNotification(value.TotalPlayerFame, value.TotalGainedFame, value.ZoneFame, value.PremiumFame, value.SatchelFame, value.IsMobFame));
            _fameCountUpTimer.AddFame(value.TotalGainedFame);

            _trackingController.SetTotalPlayerFame(Formatting.ToStringShort(value.TotalPlayerFame));
            await Task.CompletedTask;
        }
        
        private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame, double satchelFame, bool isMobFame)
        {
            //var fameText = isMobFame ? "Mob Fame" : "Tome Fame";

            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame, LanguageController.Translation("FAME"), FameTypeOperator.Pve, zoneFame, premiumFame, satchelFame, LanguageController.Translation("GAINED")),
            });
        }
    }
}