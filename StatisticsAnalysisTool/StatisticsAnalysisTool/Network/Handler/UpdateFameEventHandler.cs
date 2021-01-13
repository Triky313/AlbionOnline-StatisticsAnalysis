using Albion.Network;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly FameCountUpTimer _fameCountUpTimer;

        public UpdateFameEventHandler(MainWindowViewModel mainWindowViewModel, FameCountUpTimer fameCountUpTimer) : base(EventCodes.UpdateFame)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _fameCountUpTimer = fameCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            if (value.ZoneMultiplier <= 1)
            {
                _mainWindowViewModel.AddTrackingNotification(SetPvpFameNotification(value.TotalPlayerFame, value.TotalGainedFame, value.ZoneFame, value.PremiumFame, value.SatchelFame));
            }
            else
            {
                _mainWindowViewModel.AddTrackingNotification(SetPveFameNotification(value.TotalPlayerFame, value.TotalGainedFame, value.ZoneFame, value.PremiumFame, value.SatchelFame));
            }

            _fameCountUpTimer.AddFame(value.TotalGainedFame);
            await Task.CompletedTask;
        }
        
        private TrackingNotification SetPvpFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame, double satchelFame)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment("Du hast", AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame, "PvP Ruhm", FameTypeOperator.Pvp, zoneFame, premiumFame, satchelFame, "erhalten."),
            });
        }

        private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame, double satchelFame)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment("Du hast", AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame, "PvE Ruhm", FameTypeOperator.Pve, zoneFame, premiumFame, satchelFame, "erhalten."),
            });
        }
    }
}