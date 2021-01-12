using Albion.Network;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public UpdateFameEventHandler(MainWindowViewModel mainWindowViewModel) : base(EventCodes.UpdateFame)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            _mainWindowViewModel.AddTrackingNotification(SetNotification(value.TotalPlayerFame, value.TotalGainedFame, value.ZoneFame, value.PremiumFame, value.SatchelFame));

            EventCounter(value.TotalGainedFame);
            await Task.CompletedTask;
        }

        private double eventCounter;

        private void EventCounter(double fame)
        {
            // TODO: Fame counter debug test
            eventCounter += fame;
            Debug.Print($"Fame counter: {eventCounter:N}");
        }

        private TrackingNotification SetNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame, double satchelFame)
        {
            var test = new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FameNotificationFragment("Du hast", AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame, "Ruhm", zoneFame, premiumFame, satchelFame, "erhalten."),
                //new TextFragment("Du hast"),
                //new TotalFameStatFragment("Ruhm", AttributeStatOperator.Plus, totalFame),
                //new FameStatFragment(zoneFame, premiumFame, satchelFame),
                //new TextFragment("erhalten.")
            });
            return test;
        }
    }
}