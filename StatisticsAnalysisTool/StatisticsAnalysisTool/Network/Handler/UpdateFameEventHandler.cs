using Albion.Network;
using StatisticsAnalysisTool.Models.NetworkModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly ObservableCollection<TrackingNotification> _trackingNotifications;

        public UpdateFameEventHandler(ObservableCollection<TrackingNotification> trackingNotifications) : base(EventCodes.UpdateFame)
        {
            _trackingNotifications = trackingNotifications;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            //Debug.Print($"-----------------------------------------");
            //Debug.Print($"UpdateFame");
            //Debug.Print($"TotalFame: {value.TotalFame}");
            //Debug.Print($"FameWithZoneAndWithoutPremium: {value.FameWithZoneAndWithoutPremium}");
            //Debug.Print($"ZoneMultiplier: {value.ZoneMultiplier}");
            //Debug.Print($"NormalFame: {value.NormalFame}");
            //Debug.Print($"FameWithZoneAndPremium: {value.FameWithZoneAndPremium}");
            //Debug.Print($"PremiumFame: {value.PremiumFame}");
            //Debug.Print($"ZoneFame: {value.ZoneFame}");

            Application.Current.Dispatcher.Invoke(delegate
            {
                _trackingNotifications.Add(new TrackingNotification($"FameWithZoneAndPremium {value.FameWithZoneAndPremium}"));
            });

            EventCounter(value.FameWithZoneAndPremium);
            await Task.CompletedTask;
        }

        private double eventCounter;

        private void EventCounter(double fame)
        {
            eventCounter += fame;
            Debug.Print($"Fame counter: {eventCounter:N}");
        }
    }
}